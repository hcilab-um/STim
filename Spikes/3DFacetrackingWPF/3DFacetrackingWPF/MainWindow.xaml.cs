﻿using Microsoft.Kinect;
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

namespace _3DFacetrackingWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		const double SCREEN_WIDTH = 1.02;
        const double SCREEN_HEIGHT = 0.58;
        private KinectSensor kinectSensor;
        
        private static readonly Point3D startV = new Point3D(-0.5375, -0.29, -0.5);
        private static readonly Point3D endV = new Point3D(-0.5375, 0.29, -0.5);

        private static readonly Point3D startH = new Point3D(-0.5375, -0.29, -0.5);
        private static readonly Point3D endH = new Point3D(0.5375, -0.29, -0.5);

		public MainWindow()
		{
			InitializeComponent();
            drawGrid();
		}

        private void drawGrid()
        {
            ScreenSpaceLines3D normal0Wire = new ScreenSpaceLines3D();
            int width = 1;
            normal0Wire.Thickness = width;
            normal0Wire.Color = Colors.Black;
            Point3D a, b;
            for (int i = 0; i < 107; i++)
            {
                a = startV;
                b = endV;
                a.X += i * 0.01;
                b.X += i * 0.01;
                normal0Wire.Points.Add(a);
                normal0Wire.Points.Add(b);
            }

            for (int i = 0; i < 58; i++)
            {
                a = startH;
                b = endH;
                a.Y += i * 0.01;
                b.Y += i * 0.01;
                normal0Wire.Points.Add(a);
                normal0Wire.Points.Add(b);
            }

            normal0Wire.Points.Add(new Point3D(-0.5375,-0.289, -0.5));
            normal0Wire.Points.Add(new Point3D(-0.5375,-0.289, 0));

            normal0Wire.Points.Add(new Point3D(0.5375,-0.289, -0.5));
            normal0Wire.Points.Add(new Point3D(0.5375,-0.289, 0));

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
			Vector3D headP = new Vector3D(head.Position.X, head.Position.Y+0.4275, head.Position.Z);
            double z = headP.Z;
            if (z == 0)
                return;
            double angle = Math.Atan(SCREEN_WIDTH / 2 / z) * 180 / Math.PI*2;
            pCamera.FieldOfView = angle;
            pCamera.LookDirection = new Vector3D(-headP.X, -headP.Y, -headP.Z);
            headP.Y += 0.04;
            pCamera.Position = (Point3D)headP;
            Vector3D displacement = new Vector3D(pCamera.Position.X - headP.X, pCamera.Position.Y - headP.Y, pCamera.Position.Z - headP.Z);
            tb.Text = displacement.Length.ToString();
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
