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
	public class VisitorController: INotifyPropertyChanged
	{
		//If there is a displacement of at least the value below of in the dimension of the push then the values
		// on the two other dimensions are blocked.
		const int CLOSE_PERCENT_CONSTRAIN = 30;

		private static readonly Vector3D STANDARD_VECTOR = new Vector3D(0,0,1);
		private static readonly Vector3D kinectLocation = new Vector3D(0,0,0);
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
				DetectZone();
				OnPropertyChanged("UserDisplayDistance");
			}
		}

		public Zone Zone
		{
			get { return interactZone; }
			set 
			{
				interactZone = value;
				OnPropertyChanged("Zone");
			}
		}

		public JointType Head { get; set; }

		public VisitorController() 
		{
			standardAngleInRadian = ToolBox.AngleToRadian(90);
			interactZone = Zone.Ambient;
			IsSimulating = false;
		}

		public void DetectUserPosition(Skeleton skeleton)
		{
			if (IsSimulating)
				return;

			if (ClosePercent > CLOSE_PERCENT_CONSTRAIN)
			{
				UserDisplayDistance = Settings.Default.CloseZoneConstrain / 2;
				return;
			}

			if (skeleton != null)
			{
				Head = JointType.Head;
				Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Head);
				headLocation = new Vector3D(head.Position.X, head.Position.Y, head.Position.Z);
				double currentAngleR = ToolBox.AngleToRadian(Vector3D.AngleBetween(STANDARD_VECTOR, headLocation));
				double headDistance = ToolBox.GetDisplacementVector((Vector3D)headLocation, (Vector3D)kinectLocation).Length;

				if (headLocation.Y < 0)
				{
					UserDisplayDistance = Math.Sin(StandardAngleInRadian - currentAngleR) * headDistance - Settings.Default.Kinect_DisplayDistance;
				}
				else
				{
					UserDisplayDistance = Math.Sin(StandardAngleInRadian + currentAngleR) * headDistance - Settings.Default.Kinect_DisplayDistance;
				}
			}
			else
				UserDisplayDistance = Settings.Default.NotificationZoneConstrain;
		}

		private void DetectZone()
		{
			if (userDisplayDistance < Settings.Default.CloseZoneConstrain)
			{
				Zone = Zone.Close;
			}
			else if (userDisplayDistance >= Settings.Default.CloseZoneConstrain && userDisplayDistance < Settings.Default.InteractionZoneConstrain)
			{
				Zone = Zone.Interaction;
			}
			else if (userDisplayDistance >= Settings.Default.InteractionZoneConstrain && userDisplayDistance < Settings.Default.NotificationZoneConstrain)
			{
				Zone = Zone.Notification;
			}
			else if (userDisplayDistance >= Settings.Default.NotificationZoneConstrain)
			{
				Zone = Zone.Ambient;
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
