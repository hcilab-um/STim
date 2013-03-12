using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using STimWPF.Util;
using STimWPF.Properties;

namespace STimWPF.Interaction
{
	public class VisitorController : INotifyPropertyChanged
	{
		//If there is a displacement of at least the value below of in the dimension of the push then the values
		// on the two other dimensions are blocked.
		const int CLOSE_PERCENT_CONSTRAIN = 60;
        
		private static readonly Vector3D STANDARD_VECTOR = new Vector3D(0, 0, 1);
		private static readonly Vector3D kinectLocation = new Vector3D(0, 0, 0);
		private Vector3D headLocation;
		private double userDisplayDistance;
		private double standardAngleInRadian;
		private Zone interactZone;
		private bool isSimulating;
		private int closePercent;

		public int ClosePercent
		{
			get { return closePercent; }
			set
			{
				closePercent = value;
				OnPropertyChanged("ClosePercent");
			}
		}

		public bool IsSimulating
		{
			get { return isSimulating; }
			set
			{
				isSimulating = value;
				OnPropertyChanged("IsSimulating");
			}
		}

		public double StandardAngleInRadian
		{
			get { return standardAngleInRadian; }
			set
			{
				standardAngleInRadian = value;
				OnPropertyChanged("StandardAngleInRadian");
			}
		}

		public double UserDisplayDistance
		{
			get { return userDisplayDistance; }
			set
			{
				userDisplayDistance = value;
				OnPropertyChanged("UserDisplayDistance");
			}
		}

		public Zone Zone
		{
			get { return interactZone; }
			set
			{
                if (interactZone == value)
                    return;
				interactZone = value;
				OnPropertyChanged("Zone");
			}
		}

		public JointType Head { get; set; }

		public VisitorController()
		{
			standardAngleInRadian = ToolBox.AngleToRadian(90);
			UserDisplayDistance = Settings.Default.NotificationZoneConstrain;
			interactZone = Zone.Ambient;
			IsSimulating = false;
		}

		public double DetectSkeletonDistance(Skeleton skeleton)
		{
			if (skeleton == null)
				throw new Exception("in DetectSkeletonDistance, skeleton can not be null.");

			Head = JointType.Head;
			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Head);
			headLocation = new Vector3D(head.Position.X, head.Position.Y, head.Position.Z);
			double currentAngleRadian = ToolBox.AngleToRadian(Vector3D.AngleBetween(STANDARD_VECTOR, headLocation));
			double headDistance = ToolBox.GetDisplacementVector((Vector3D)headLocation, (Vector3D)kinectLocation).Length;

			if (headLocation.Y < 0)
				return Math.Sin(StandardAngleInRadian - currentAngleRadian) * headDistance - Settings.Default.Kinect_DisplayDistance;

			return Math.Sin(StandardAngleInRadian + currentAngleRadian) * headDistance - Settings.Default.Kinect_DisplayDistance;
		}

		private double DetectUserPosition(Skeleton skeleton)
		{
			if (ClosePercent > CLOSE_PERCENT_CONSTRAIN)
			{
				return Settings.Default.CloseZoneConstrain / 2;
			}

			if (skeleton != null)
			{
				return DetectSkeletonDistance(skeleton);
			}

			return Settings.Default.NotificationZoneConstrain;
		}

		public void DetectZone(Skeleton skeleton)
		{
			if (!IsSimulating)
			{
				UserDisplayDistance = DetectUserPosition(skeleton);
			}

			if (userDisplayDistance < Settings.Default.CloseZoneConstrain)
			{
				Zone = Zone.Close;
				return;
			}

			if (userDisplayDistance >= Settings.Default.CloseZoneConstrain && userDisplayDistance < Settings.Default.InteractionZoneConstrain)
			{
				Zone = Zone.Interaction;
				return;
			}

			if (userDisplayDistance >= Settings.Default.InteractionZoneConstrain && userDisplayDistance < Settings.Default.NotificationZoneConstrain)
			{
				Zone = Zone.Notification;
				return;
			}

			if (userDisplayDistance >= Settings.Default.NotificationZoneConstrain)
			{
				Zone = Zone.Ambient;
				return;
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
