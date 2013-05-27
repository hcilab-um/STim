using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using SpikeWPF.Properties;
//using Microsoft.Kinect.Toolkit.FaceTracking;

namespace SpikeWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		const double SCREEN_WIDTH = 1.02;
		const double SCREEN_HEIGHT = 0.58;

		private const float RenderWidth = 640.0f;
		private const float RenderHeight = 480.0f;
		
		private KinectSensor kinectSensor;
		//private FaceTracker faceTracker;

		private byte[] colorImage;
		private short[] depthImage;
		private Skeleton[] skeletonData;
		private SkeletonDrawer skeletonDrawer;

		private Vector3D headV = new Vector3D(0, 0, Settings.Default.NotificationZoneConstrain);

		public Vector3D HeadV
		{
			get { return headV; }
			set
			{
				headV = value;
				OnPropertyChanged("HeadV");
			}
		}

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded_1(object sender, RoutedEventArgs e)
		{
			if (KinectSensor.KinectSensors.Count == 0)
			{
				Console.WriteLine("No Kinect Found");
			}
			else
			{
				kinectSensor = KinectSensor.KinectSensors[0];
				if (kinectSensor == null || kinectSensor.Status == KinectStatus.NotPowered)
					return;

				kinectSensor.DepthStream.Enable();
				kinectSensor.ColorStream.Enable();
				kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters() { Correction = 0.5f, JitterRadius = 0.05f, MaxDeviationRadius = 0.05f, Prediction = 0.5f, Smoothing = 0.5f });

				kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinectSensor_AllFramesReady);
				this.depthImage = new short[kinectSensor.DepthStream.FramePixelDataLength];
				this.colorImage = new byte[kinectSensor.ColorStream.FramePixelDataLength];
				this.skeletonData = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
				kinectSensor.Start();
			}
		}

		void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{

			//using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
			//{
			//  if (colorImageFrame == null)
			//    return;
			//  colorImageFrame.CopyPixelDataTo(this.colorImage);
			//}

			//using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
			//{
			//  if (depthImageFrame == null)
			//    return;
			//  depthImageFrame.CopyPixelDataTo(this.depthImage);
			//}

			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame == null)
					return;
				skeletonFrame.CopySkeletonDataTo(skeletonData);
			}

			Skeleton rawSkeleton = null;
			for (int i = 0; i < skeletonData.Length; i++)
			{
				if (skeletonData[i].TrackingState != SkeletonTrackingState.NotTracked)
				{
					if (rawSkeleton == null)
					{
						rawSkeleton = skeletonData[i];
					}
					else if (rawSkeleton.Position.Z > skeletonData[i].Position.Z)
					{
						rawSkeleton = skeletonData[i];
					}
				}
			}

			ProcessSkeleton(rawSkeleton);
			
			//FaceTrackFrame faceFrame = faceTracker.Track(kinectSensor.ColorStream.Format, colorImage, kinectSensor.DepthStream.Format, depthImage, rawSkeleton);
			//if (faceFrame.TrackSuccessful)
			//{
			//  FacePoints = faceFrame.Get3DShape();
			//  Vector3DF eye = FacePoints[FeaturePoint.RightOfRightEyebrow];
			//  HeadV = new Vector3D(eye.X, eye.Y, eye.Z);
			//}
		}

		private void ProcessSkeleton(Skeleton skeleton)
		{
			if (skeleton == null)
			{
				HeadV = new Vector3D(0, 0, 10);
				return;
			}
			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == JointType.Head);

			if (head != null && head.TrackingState == JointTrackingState.Tracked)
				HeadV = new Vector3D(head.Position.X, head.Position.Y + 0.39, head.Position.Z);
		}

		private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (null != this.kinectSensor)
				this.kinectSensor.Stop();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		//drawing color image and skeleton
		private DrawingImage DrawColorImage(ColorImageFrame colorFrame, Skeleton skeleton)
		{
			DrawingGroup dgColorImageAndSkeleton = new DrawingGroup();
			DrawingImage drawingImage = new DrawingImage(dgColorImageAndSkeleton);
			skeletonDrawer = new SkeletonDrawer(kinectSensor);
			using (DrawingContext drawingContext = dgColorImageAndSkeleton.Open())
			{
				InitializeColorImage(colorFrame, drawingContext);
				skeletonDrawer.DrawFullSkeleton(skeleton, drawingContext);
			}

			//Make sure the image remains within the defined width and height
			dgColorImageAndSkeleton.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));

			return drawingImage;
		}

		private static void InitializeColorImage(ColorImageFrame colorFrame, DrawingContext drawingContext)
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
	}
}
