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
		private static KinectSensor kinectSensor;

		public event EventHandler<ColorImageReadyArgs> ColorImageReady;

		private SkeletonDrawer skeletonDrawer;
		private static Core instance;

		private Vector3D headV = new Vector3D(0, 0, Settings.Default.NotificationZoneConstrain);

		private List<Skeleton> skeletons;
		private Dictionary<Skeleton, double> skeletonAttentionList;

		public Vector3D HeadV
		{
			get { return headV; }
			set
			{
				headV = value;
				OnPropertyChanged("HeadV");
			}
		}

		public AttentionEstimator AttentionE { get; set; }

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
					skeletonAttentionList = new Dictionary<Skeleton, double>();
					skeletons = new List<Skeleton>();
					AttentionE = new AttentionEstimator();
					IsKinectConnected = true;
					kinectSensor.ColorStream.Enable();
					kinectSensor.SkeletonStream.Enable();
					kinectSensor.Start();
					kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinectSensor_AllFramesReady);
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
				skeletons = rawSkeletons.Where(temp => temp.TrackingState == SkeletonTrackingState.Tracked).ToList();
				ProcessSkeleton(skeletons);
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

		private const float RenderWidth = 640.0f;
		private const float RenderHeight = 480.0f;
		
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
						headP.Y -= 50;
						//FormattedText
						drawingContext.DrawText( 
							new FormattedText(skeletonAttentionList[skeleton].ToString(), CultureInfo.GetCultureInfo("en-us"),
																FlowDirection.LeftToRight, new Typeface("Verdana"),
																20, System.Windows.Media.Brushes.Yellow),
							headP);
					}
				}
 			}

			//Make sure the image remains within the defined width and height
			dgColorImageAndSkeleton.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
			return drawingImage;
		}

		private void ProcessSkeleton(List<Skeleton> skeletons)
		{
			if (skeletons.Count ==0)
				return;

			//double attention = AttentionE.CalculateSocialEffect(null, skeletons);
			
			foreach (Skeleton skel in skeletons)
			{
				double attention = AttentionE.CalculateAttention(skel, skeletons);
				//double attention = AttentionE.CalculateOrientationAngle(skel);
				//double attention = AttentionE.CalculateSocialEffect(skel, skeletons);
				skeletonAttentionList.Add(skel, Math.Round(attention, 2));
			}

			Skeleton skeleton = null;

			//find closest skeleton and playerIndex. 
			//Idea from: http://stackoverflow.com/questions/13847046/getuserpixels-alternative-in-official-kinect-sdk/13849204#13849204
			foreach (Skeleton skel in skeletons)
			{
					if (skeleton == null)
					{
						skeleton  = skel;
					}
					else if (skeleton.Position.Z > skel.Position.Z)
					{
						skeleton  = skel;
					}
			}

			if (skeleton == null)
			{
				HeadV = new Vector3D(0, 0, 10);
				return;
			}

			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == JointType.Head);

			if (head != null && head.TrackingState == JointTrackingState.Tracked)
				HeadV = new Vector3D(head.Position.X, head.Position.Y + 0.39, head.Position.Z);
		}

		private static void InitializeDrawingImage(ColorImageFrame colorFrame, DrawingContext drawingContext)
		{
			if (colorFrame != null)
			{
				byte[] cbyte = new byte[colorFrame.PixelDataLength];
				colorFrame.CopyPixelDataTo(cbyte);
				int stride = colorFrame.Width * 4;

				ImageSource imageBackground = BitmapSource.Create(640, 480, 96, 96, PixelFormats.Bgr32, null, cbyte, stride);
				drawingContext.DrawImage(imageBackground, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
			}
			else
			{
				// Draw a transparent background to set the render size
				drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
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
