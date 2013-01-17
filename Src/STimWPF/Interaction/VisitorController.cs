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
		private static readonly Vector3D STANDARD_VECTOR = new Vector3D(0,0,1);
		private static readonly Vector3D kinectLocation = new Vector3D(0,0,0);
		private Vector3D headLocation;
		private double userDisplayDistance;
		private double standardAngleInRadian;
		private InteractionZone interactZone;
		private bool isSimulating;
		
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

		public InteractionZone InteractionZone
		{
			get { return interactZone; }
			set 
			{
				interactZone = value;
				OnPropertyChanged("InteractionZone");
			}
		}

		public JointType Head { get; set; }

		public VisitorController() 
		{
			standardAngleInRadian = ToolBox.AngleToRadian(90);
			interactZone = InteractionZone.Ambient;
			IsSimulating = false;
		}

		public void DetectUserPosition(Skeleton skeleton)
		{
			if (IsSimulating)
				return;
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

		private void DetectZone()
		{
			if (userDisplayDistance < Settings.Default.CloseZoneConstrain)
			{
				InteractionZone = InteractionZone.Close;
			}
			else if (userDisplayDistance >= Settings.Default.CloseZoneConstrain && userDisplayDistance < Settings.Default.InteractionZoneConstrain)
			{
				InteractionZone = InteractionZone.Interaction;
			}
			else if (userDisplayDistance >= Settings.Default.InteractionZoneConstrain && userDisplayDistance < Settings.Default.NotificationZoneConstrain)
			{
				InteractionZone = InteractionZone.Notification;
			}
			else if (userDisplayDistance >= Settings.Default.NotificationZoneConstrain)
			{
				InteractionZone = InteractionZone.Ambient;
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
