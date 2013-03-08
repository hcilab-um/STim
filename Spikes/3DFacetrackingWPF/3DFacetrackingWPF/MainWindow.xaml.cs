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

namespace _3DFacetrackingWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private KinectSensor kinectSensor;

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
				kinectSensor.SkeletonStream.Enable();
				kinectSensor.Start();
				kinectSensor.SkeletonFrameReady += this.Kinect_SkeletonFrameReady;
			}
		}

		private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
		{
			Skeleton[] skeletons = null;
			Skeleton skeleton = null;
			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame != null)
				{
					skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
					skeletonFrame.CopySkeletonDataTo(skeletons);

					//find closest skeleton and playerIndex. 
					//Idea from: http://stackoverflow.com/questions/13847046/getuserpixels-alternative-in-official-kinect-sdk/13849204#13849204
					for (int i = 0; i < skeletons.Length; i++)
					{
						if (skeletons[i].TrackingState != SkeletonTrackingState.NotTracked)
						{
							if (skeleton == null)
							{
								skeleton = skeletons[i];
							}
							else if (skeleton.Position.Z > skeletons[i].Position.Z)
							{
								skeleton = skeletons[i];
							}
						}
					}
				}
			}
			if (skeleton != null)
			{
				ProcessSkeleton(skeleton);
			}
		}

		private void ProcessSkeleton(Skeleton skeleton)
		{
			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == JointType.Head);
			Point3D headP = new Point3D(head.Position.X, head.Position.Y+0.14, head.Position.Z+0.70);
			pCamera.Position = headP;
			pCamera.LookDirection = new Vector3D(-headP.X, -headP.Y, -headP.Z);
		}

		private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (null != this.kinectSensor)
			{
				this.kinectSensor.Stop();
			}
		}

	}
}
