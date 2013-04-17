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
using _3DTools;
using System.ComponentModel;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace KinectWPF3D
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		const double SCREEN_WIDTH = 1.02;
		const double SCREEN_HEIGHT = 0.58;

		private KinectSensor kinectSensor;
		private FaceTracker faceTracker;

		private byte[] colorImage;
		private short[] depthImage;
		private Skeleton[] skeletonData;

		private static readonly Point3D startV = new Point3D(-0.5375, -0.29, -0.5);
		private static readonly Point3D endV = new Point3D(-0.5375, 0.29, -0.5);

		private static readonly Point3D startH = new Point3D(-0.5375, -0.29, -0.5);
		private static readonly Point3D endH = new Point3D(0.5375, -0.29, -0.5);
		
		private EnumIndexableCollection<FeaturePoint, Vector3DF> FacePoints;
		
		private Vector3D headV;

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
			//drawGrid();
		}

		private void drawGrid()
		{
			ScreenSpaceLines3D normal0Wire = new ScreenSpaceLines3D();
			int width = 2;
			normal0Wire.Thickness = width;
			normal0Wire.Color = Colors.Red;
			Point3D a, b;
			for (int i = 0; i <= 107; i++)
			{
				a = startV;
				b = endV;
				a.X += i * 0.01;
				b.X += i * 0.01;
				normal0Wire.Points.Add(a);
				normal0Wire.Points.Add(b);
			}

			for (int i = 0; i <= 58; i++)
			{
				a = startH;
				b = endH;
				a.Y += i * 0.01;
				b.Y += i * 0.01;
				normal0Wire.Points.Add(a);
				normal0Wire.Points.Add(b);
			}

			normal0Wire.Points.Add(new Point3D(-0.5375, -0.289, -0.5));
			normal0Wire.Points.Add(new Point3D(-0.5375, -0.289, 0));

			normal0Wire.Points.Add(new Point3D(0.5375, -0.289, -0.5));
			normal0Wire.Points.Add(new Point3D(0.5375, -0.289, 0));

			normal0Wire.Points.Add(new Point3D(-0.5375, 0.289, -0.5));
			normal0Wire.Points.Add(new Point3D(-0.5375, 0.289, 0));

			normal0Wire.Points.Add(new Point3D(0.5375, 0.289, -0.5));
			normal0Wire.Points.Add(new Point3D(0.5375, 0.289, 0));

			viewport.Children.Add(normal0Wire);
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
				faceTracker = new FaceTracker(kinectSensor);
			}
		}

		void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{

			using(ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
			{
				if (colorImageFrame == null)
					return;
				colorImageFrame.CopyPixelDataTo(this.colorImage);
			}

			using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
			{
				if (depthImageFrame == null)
					return;
				depthImageFrame.CopyPixelDataTo(this.depthImage);
			}

			using(SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
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

			FaceTrackFrame faceFrame = faceTracker.Track(kinectSensor.ColorStream.Format, colorImage, kinectSensor.DepthStream.Format, depthImage, rawSkeleton);
			if (faceFrame.TrackSuccessful)
			{
				FacePoints = faceFrame.Get3DShape();
				Vector3DF eye = FacePoints[FeaturePoint.RightOfRightEyebrow];
				HeadV = new Vector3D(eye.X, eye.Y, eye.Z);
			}

		}

		private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (null != this.kinectSensor)
			{
				this.kinectSensor.Stop();
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
