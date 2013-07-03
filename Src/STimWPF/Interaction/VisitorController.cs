using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using STimWPF.Properties;
using STimWPF.Util;

namespace STimWPF.Interaction
{
	public class VisitorController : INotifyPropertyChanged
	{
		//If there is a displacement of at least the value below of in the dimension of the push then the values
		// on the two other dimensions are blocked.
		const int CLOSE_PERCENT_CONSTRAIN = 60;

		private static readonly Vector3D STANDARD_VECTOR = new Vector3D(0, 0, 1);
		private static readonly Vector3D kinectLocation = new Vector3D(0, 0, 0);
		private double userDisplayDistance;
		private double standardAngleInRadian;
		private Zone interactZone;
		private bool isSimulating;

		private double closePercent;
		public double ClosePercent
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

		private double DetectUserPosition(WagSkeleton skeleton)
		{
			if (ClosePercent > CLOSE_PERCENT_CONSTRAIN)
			{
				return Settings.Default.CloseZoneConstrain / 2;
			}

			if (skeleton != null)
			{
				return skeleton.TransformedJoints[JointType.Head].Position.Z;
			}

			return Settings.Default.NotificationZoneConstrain;
		}

		public Zone DetectZone(WagSkeleton skeleton)
		{
			Zone calculatedZone = Zone.Ambient;
			if (!IsSimulating)
				UserDisplayDistance = DetectUserPosition(skeleton);

			if (UserDisplayDistance < Settings.Default.CloseZoneConstrain)
				calculatedZone = Zone.Close;
			else if (UserDisplayDistance >= Settings.Default.CloseZoneConstrain && userDisplayDistance < Settings.Default.InteractionZoneConstrain)
				calculatedZone = Zone.Interaction;
			else if (UserDisplayDistance >= Settings.Default.InteractionZoneConstrain && userDisplayDistance < Settings.Default.NotificationZoneConstrain)
				calculatedZone = Zone.Notification;
			else if (UserDisplayDistance >= Settings.Default.NotificationZoneConstrain)
				calculatedZone = Zone.Ambient;

			return calculatedZone;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
