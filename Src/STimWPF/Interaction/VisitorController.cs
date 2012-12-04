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
		private const double CLOSE_CONSTRAIN = 2.03;
		private const double INTERACTION_CONSTRAIN = 3.15;
		private const double NOTIFICATION = 4.28;
		
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

		public VisitorController()
		{
 
		}

		public void DetectUserPosition(Skeleton skeleton)
		{
			Head = JointType.Head;
			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Head);
			Point3D kinectLocation = new Point3D();
			Point3D headLocation = new Point3D(head.Position.X, head.Position.Y, head.Position.Z);
			double userDistance = ToolBox.GetDisplacementVector((System.Windows.Media.Media3D.Vector3D)headLocation, (System.Windows.Media.Media3D.Vector3D)kinectLocation).Length;
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
