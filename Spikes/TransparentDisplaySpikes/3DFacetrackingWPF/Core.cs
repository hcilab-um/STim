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

namespace SpikeWPF
{
	public class Core : INotifyPropertyChanged
	{
		const int MINIMUM_JOINT_THRESHOLD = 7;

		private static KinectSensor kinectSensor;

		public event EventHandler<ColorImageReadyArgs> ColorImageReady;

		private SkeletonDrawer skeletonDrawer;
		private static Core instance;

		private Vector3D headV = new Vector3D(0, 0, Settings.Default.NotificationZoneConstrain);

		private List<Skeleton> skeletons;
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
						Smoothing = 0.7f,
						Correction = 0.1f,
						Prediction = 0.1f,
						JitterRadius = 0.05f,
						MaxDeviationRadius = 0.04f
					};

					kinectSensor.SkeletonStream.Enable(smoothParameters);
					kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
					kinectSensor.Start();
	
					kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinectSensor_AllFramesReady);

					skeletonAttentionSocialList = new Dictionary<int, AttentionSocial>();
					skeletonAttentionSimpleList = new Dictionary<int, AttentionSimple>();
					skeletons = new List<Skeleton>();

					AttentionEstimatorSocial = new AttentionEstimatorSocial();
					AttentionEstimatorSimple = new AttentionEstimatorSimple();
				}
			}
		}

		void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			Skeleton[] rawSkeletons = null;
			using(SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
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
					DrawingImage imageCanvas = DrawImage(colorFrame, skeletons);
					ColorImageReady(this, new ColorImageReadyArgs() { Frame = imageCanvas });
				}
			}
		}

		private List<Skeleton> ExtractValidSkeletons(Skeleton[] rawSkeletons)
		{
			
			var skList = rawSkeletons.Where(skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked);
			skList = skList.Where(skeleton => skeleton.Joints.Count(joint => joint.TrackingState == JointTrackingState.Tracked) > MINIMUM_JOINT_THRESHOLD);
			return skList.ToList();
		}

		private void ProcessSkeletons(List<Skeleton> skeletons)
		{
			if (skeletons.Count == 0)
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
			skeletons.Sort((x, y) => (int)(x.Position.Z - y.Position.Z));
			Skeleton closestSkeleton = skeletons.FirstOrDefault();

			if (closestSkeleton == null)
			{
				HeadV = new Vector3D(0, 0, 10);
				return;
			}

			Joint head = closestSkeleton.Joints.SingleOrDefault(tmp => tmp.JointType == JointType.Head);
			if (head != null && head.TrackingState == JointTrackingState.Tracked)
				HeadV = new Vector3D(head.Position.X, head.Position.Y + 0.39, head.Position.Z);
		}

		private const float RENDER_WIDTH = 640.0f;
		private const float RENDER_HEIGHT = 480.0f;
		
		private DrawingImage DrawImage(ColorImageFrame colorFrame, List<Skeleton> skeletons)
		{
			DrawingGroup dgColorImageAndSkeleton = new DrawingGroup();
			DrawingImage drawingImage = new DrawingImage(dgColorImageAndSkeleton);
			skeletonDrawer = new SkeletonDrawer(kinectSensor);

			using (DrawingContext drawingContext = dgColorImageAndSkeleton.Open())
			{
				InitializeDrawingImage(colorFrame, drawingContext);
				if (skeletons.Count != 0)
				{
					foreach (Skeleton skeleton in skeletons)
					{
						skeletonDrawer.DrawUpperSkeleton(skeleton, drawingContext);
						Joint head = skeleton.Joints.SingleOrDefault(temp => temp.JointType == JointType.Head);
						Point headP = skeletonDrawer.SkeletonPointToScreen(head.Position);
						Point socialDataP = headP;
						socialDataP.Y = headP.Y - 50;

						Point simpleDataP = headP;
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
			dgColorImageAndSkeleton.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RENDER_WIDTH, RENDER_HEIGHT));
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
				drawingContext.DrawImage(imageBackground, new Rect(0.0, 0.0, RENDER_WIDTH, RENDER_HEIGHT));
			}
			else
			{
				// Draw a transparent background to set the render size
				drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RENDER_WIDTH, RENDER_HEIGHT));
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
