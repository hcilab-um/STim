using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using SpikeWPF.Properties;
using System.Globalization;
using SpikeWPF.Attention;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace SpikeWPF
{
	public class Core : INotifyPropertyChanged
	{
		const int MINIMUM_JOINT_THRESHOLD = 15;

		private static KinectSensor kinectSensor;
		private static Core instance;

		public event EventHandler<ColorImageReadyArgs> ColorImageReady;

		private SkeletonDrawer skeletonDrawer;

		private Dictionary<int, FaceTracker> faceTrackers = new Dictionary<int, FaceTracker>();
		private Dictionary<int, FaceTrackFrame> faceFrames = new Dictionary<int, FaceTrackFrame>();

		private byte[] colorImage;
		private short[] depthImage;

		private Vector3D headV = new Vector3D(0, 0, Settings.Default.NotificationZoneConstrain);

		private Skeleton[] skeletons;
		private Dictionary<int, AttentionSocial> skeletonAttentionSocialList;
		private Dictionary<int, AttentionSimple> skeletonAttentionSimpleList;

		public Vector3D HeadV
		{
			get { return headV; }
			set
			{
				headV = value;
				OnPropertyChanged("HeadV");
			}
		}

		public AttentionEstimatorSocial AttentionEstimatorSocial { get; set; }
		public AttentionEstimatorSimple AttentionEstimatorSimple { get; set; }

		public bool IsKinectConnected { get; set; }

		public static Core Instance
		{
			get
			{
				if (instance == null)
					instance = new Core();
				return instance;
			}
		}

		private Core() { }

		public void Initialize()
		{
			if (KinectSensor.KinectSensors.Count == 0)
			{
				IsKinectConnected = false;
				Console.WriteLine("No Kinect found");
			}
			else
			{
				IsKinectConnected = true;
				kinectSensor = KinectSensor.KinectSensors[0];
				if (kinectSensor == null)
				{
					IsKinectConnected = false;
					throw new Exception("Kinect Not detected");
				}
				else
				{
					IsKinectConnected = true;
					kinectSensor.ColorStream.Enable();
					TransformSmoothParameters smoothParameters = new TransformSmoothParameters()
					{
						Smoothing = 0.9f,
						Correction = 0.01f,
						Prediction = 0.1f,
						JitterRadius = 0.01f,
						MaxDeviationRadius = 0.01f
					};

					kinectSensor.SkeletonStream.Enable(smoothParameters);
					kinectSensor.DepthStream.Enable();
					kinectSensor.ColorStream.Enable();
					kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
					kinectSensor.Start();

					kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinectSensor_AllFramesReady);

					skeletonAttentionSocialList = new Dictionary<int, AttentionSocial>();
					skeletonAttentionSimpleList = new Dictionary<int, AttentionSimple>();
					skeletons = null;

					AttentionEstimatorSocial = new AttentionEstimatorSocial();
					AttentionEstimatorSimple = new AttentionEstimatorSimple();
				}
			}
		}

		void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			Skeleton[] rawSkeletons = null;

			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame != null)
				{
					depthImage = new short[depthFrame.PixelDataLength];
					depthFrame.CopyPixelDataTo(depthImage);
				}
			}

			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame != null)
				{
					rawSkeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
					skeletonFrame.CopySkeletonDataTo(rawSkeletons);
				}
			}

			if (rawSkeletons != null)
			{
				skeletons = ExtractValidSkeletons(rawSkeletons);
				ProcessSkeletons(skeletons);
			}

			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame != null)
				{
					colorImage = new byte[colorFrame.PixelDataLength];
					colorFrame.CopyPixelDataTo(colorImage);
					DrawingImage imageCanvas = DrawImage(colorFrame, skeletons);
					ColorImageReady(this, new ColorImageReadyArgs() { Frame = imageCanvas });
				}
			}

			if (skeletons == null)
				return;

			// Update the list of trackers and the trackers with the current frame information
			foreach (Skeleton skeleton in this.skeletons)
			{
				// We want keep a record of any skeleton, tracked or untracked.
				if (!this.faceTrackers.ContainsKey(skeleton.TrackingId))
				{
					this.faceTrackers.Add(skeleton.TrackingId, new FaceTracker(kinectSensor));
					
				}

				FaceTrackFrame faceFrame = null;

				// Give each tracker the upated frame.
				FaceTracker faceTracker;
				if (this.faceTrackers.TryGetValue(skeleton.TrackingId, out faceTracker))
				{
					faceFrame = faceTracker.Track(kinectSensor.ColorStream.Format, colorImage, kinectSensor.DepthStream.Format, depthImage, skeleton);
				}

				if (faceFrame != null)
				{
					if (this.faceFrames.ContainsKey(skeleton.TrackingId))
						this.faceFrames[skeleton.TrackingId] = faceFrame;
					else
						this.faceFrames.Add(skeleton.TrackingId, faceFrame);
				}
			}

		}

		private Skeleton[] ExtractValidSkeletons(Skeleton[] rawSkeletons)
		{

			var skList = rawSkeletons.Where(skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked);
			skList = skList.Where(skeleton => skeleton.Joints.Count(joint => joint.TrackingState == JointTrackingState.Tracked) > MINIMUM_JOINT_THRESHOLD);
			return skList.ToArray();
		}

		private void ProcessSkeletons(Skeleton[] skeletons)
		{
			if (skeletons == null || skeletons.Count() == 0)
				return;

			foreach (Skeleton skel in skeletons)
			{
				AttentionSocial attentionSocial = AttentionEstimatorSocial.CalculateAttention(skel, skeletons);
				if (skeletonAttentionSocialList.ContainsKey(skel.TrackingId))
					skeletonAttentionSocialList[skel.TrackingId] = attentionSocial;
				else
					skeletonAttentionSocialList.Add(skel.TrackingId, attentionSocial);

				AttentionSimple attentionSimple = AttentionEstimatorSimple.CalculateAttention(skel);
				if (skeletonAttentionSimpleList.ContainsKey(skel.TrackingId))
					skeletonAttentionSimpleList[skel.TrackingId] = attentionSimple;
				else
					skeletonAttentionSimpleList.Add(skel.TrackingId, attentionSimple);
			}

			//find closest skeleton and playerIndex. 
			skeletons.OrderBy(skeleton => skeleton.Position.Z);
			Skeleton closestSkeleton = skeletons.FirstOrDefault();

			if (closestSkeleton == null)
			{
				HeadV = new Vector3D(0, 0, 10);
				return;
			}

			FaceTrackFrame closestFace;
			if (this.faceFrames.TryGetValue(closestSkeleton.TrackingId, out closestFace) && closestFace.TrackSuccessful)
			{
				var FacePoints = closestFace.Get3DShape();
				Vector3DF leftEyeInner = FacePoints[FeaturePoint.InnerCornerLeftEye];
				Vector3DF leftEyeOuter = FacePoints[FeaturePoint.OuterCornerOfLeftEye];
				HeadV = new Vector3D((leftEyeInner.X + leftEyeOuter.X) / 2, (leftEyeInner.Y + leftEyeOuter.Y) / 2 + 0.39, (leftEyeInner.Z + leftEyeOuter.Z) / 2);
				return;
			}

			Joint head = closestSkeleton.Joints.SingleOrDefault(tmp => tmp.JointType == JointType.Head);
			if (head != null && head.TrackingState == JointTrackingState.Tracked)
				HeadV = new Vector3D(head.Position.X, head.Position.Y + 0.39, head.Position.Z);
		}

		private const float RENDER_WIDTH = 640.0f;
		private const float RENDER_HEIGHT = 480.0f;

		private DrawingImage DrawImage(ColorImageFrame colorFrame, Skeleton[] skeletons)
		{
			DrawingGroup dgColorImageAndSkeleton = new DrawingGroup();
			DrawingImage drawingImage = new DrawingImage(dgColorImageAndSkeleton);
			skeletonDrawer = new SkeletonDrawer(kinectSensor);


			using (DrawingContext drawingContext = dgColorImageAndSkeleton.Open())
			{
				InitializeDrawingImage(colorFrame, drawingContext);

				if (skeletons != null && skeletons.Count() > 0)
				{
					foreach (Skeleton skeleton in skeletons)
					{
						skeletonDrawer.DrawFullSkeleton(skeleton, drawingContext);
						Joint head = skeleton.Joints.SingleOrDefault(temp => temp.JointType == JointType.Head);
						System.Windows.Point headP = skeletonDrawer.SkeletonPointToScreen(head.Position);
						System.Windows.Point socialDataP = headP;
						socialDataP.Y = headP.Y - 50;

						System.Windows.Point simpleDataP = headP;
						simpleDataP.Y = headP.Y - 90;

						//FormattedText
						drawingContext.DrawText(
							new FormattedText(skeletonAttentionSocialList[skeleton.TrackingId].ToString(), CultureInfo.GetCultureInfo("en-us"),
																FlowDirection.LeftToRight, new Typeface("Verdana"),
																20, System.Windows.Media.Brushes.Green),
							socialDataP);
						drawingContext.DrawText(
							new FormattedText(skeletonAttentionSimpleList[skeleton.TrackingId].ToString(), CultureInfo.GetCultureInfo("en-us"),
										FlowDirection.LeftToRight, new Typeface("Verdana"), 20, System.Windows.Media.Brushes.Green),
							simpleDataP);
					}
				}

			}

			//Make sure the image remains within the defined width and height
			dgColorImageAndSkeleton.ClipGeometry = new RectangleGeometry(new System.Windows.Rect(0.0, 0.0, RENDER_WIDTH, RENDER_HEIGHT));
			return drawingImage;
		}

		private static void InitializeDrawingImage(ColorImageFrame colorFrame, DrawingContext drawingContext)
		{
			if (colorFrame != null)
			{
				byte[] cbyte = new byte[colorFrame.PixelDataLength];
				colorFrame.CopyPixelDataTo(cbyte);
				int stride = colorFrame.Width * 4;

				ImageSource imageBackground = BitmapSource.Create(640, 480, 96, 96, PixelFormats.Bgr32, null, cbyte, stride);
				drawingContext.DrawImage(imageBackground, new System.Windows.Rect(0.0, 0.0, RENDER_WIDTH, RENDER_HEIGHT));
			}
			else
			{
				// Draw a transparent background to set the render size
				drawingContext.DrawRectangle(Brushes.Black, null, new System.Windows.Rect(0.0, 0.0, RENDER_WIDTH, RENDER_HEIGHT));
			}
		}

		public void Shutdown()
		{
			if (kinectSensor != null)
			{
				kinectSensor.Stop();
				kinectSensor.Dispose();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

	}
}
