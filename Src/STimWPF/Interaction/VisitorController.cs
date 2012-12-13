using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using STimWPF.Util;

namespace STimWPF.Interaction
{
	public class VisitorController: INotifyPropertyChanged
	{
		//If there is a displacement of at least the value below of in the dimension of the push then the values
		// on the two other dimensions are blocked.
		private const double CLOSE_CONSTRAIN = 0.5;
		private const double INTERACTION_CONSTRAIN = 1.0;
		private const double NOTIFICATION = 2.0;
		private readonly double STANDARD_ANGLE_R = ToolBox.AngleToRadian(90 - 29);
		private static readonly Vector3D STANDARD_VECTOR = new Vector3D(0,0,1);
		private static readonly Vector3D kinectLocation = new Vector3D(0,0,0);
		private Vector3D headLocation;
		private double userDistance;
		private InteractionZone interactZone = InteractionZone.Ambient;
		
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

		public VisitorController() { }

		public void DetectUserPosition(Skeleton skeleton)
		{
			Head = JointType.Head;
			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Head);
			headLocation = new Vector3D(head.Position.X, head.Position.Y, head.Position.Z);
			double currentAngleR = ToolBox.AngleToRadian(Vector3D.AngleBetween(STANDARD_VECTOR, headLocation));
			double headDistance = ToolBox.GetDisplacementVector((Vector3D)headLocation, (Vector3D)kinectLocation).Length;
			
			if (headLocation.Y < 0)
			{
				userDistance = Math.Sin(STANDARD_ANGLE_R - currentAngleR) * headDistance;
			}
			else
			{
				userDistance = Math.Sin(STANDARD_ANGLE_R + currentAngleR) * headDistance;
			}

			if (userDistance < CLOSE_CONSTRAIN)
			{
				InteractionZone = InteractionZone.Close;
			}
			else if (userDistance >= CLOSE_CONSTRAIN && userDistance < INTERACTION_CONSTRAIN)
			{
				InteractionZone = InteractionZone.Interaction;
			}
			else if (userDistance >= INTERACTION_CONSTRAIN && userDistance < NOTIFICATION)
			{
				InteractionZone = InteractionZone.Notification;
			}
			else if (userDistance >= NOTIFICATION)
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
