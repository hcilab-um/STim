using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using STim.Util;

namespace STim.Interaction
{
	public class VisitorController : INotifyPropertyChanged
	{

		private static readonly Vector3D STANDARD_VECTOR = new Vector3D(0, 0, 1);
		private static readonly Vector3D kinectLocation = new Vector3D(0, 0, 0);
		private double userDisplayDistance;
		private double standardAngleInRadian;
		private Zone zone;
		private bool isSimulating;
		private double closePercent;
		public bool IsBlocked
		{
			get { return (closePercent > STimSettings.BlockDepthPercent); }
		}

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
			get { return zone; }
			set
			{
				if (zone == value)
					return;
				zone = value;
				OnPropertyChanged("Zone");
			}
		}

		public JointType Head { get; set; }

		public VisitorController()
		{
			standardAngleInRadian = ToolBox.AngleToRadian(90);
			UserDisplayDistance = STimSettings.NotificationZoneConstrain;
			zone = Zone.Ambient;
			IsSimulating = false;
		}

		private double DetectUserPosition(WagSkeleton skeleton)
		{
			if (ClosePercent > STimSettings.BlockDepthPercent)
			{
				return STimSettings.CloseZoneConstrain / 2;
			}

			if (skeleton != null)
			{
				return skeleton.TransformedJoints[JointType.Head].Position.Z;
			}

			return STimSettings.NotificationZoneConstrain;
		}

		public Zone DetectZone(WagSkeleton skeleton)
		{
			Zone calculatedZone = Zone.Ambient;
			if (!IsSimulating)
				UserDisplayDistance = DetectUserPosition(skeleton);

			if (UserDisplayDistance < STimSettings.CloseZoneConstrain)
				calculatedZone = Zone.Close;
			else if (UserDisplayDistance >= STimSettings.CloseZoneConstrain && userDisplayDistance < STimSettings.InteractionZoneConstrain)
				calculatedZone = Zone.Interaction;
			else if (UserDisplayDistance >= STimSettings.InteractionZoneConstrain && userDisplayDistance < STimSettings.NotificationZoneConstrain)
				calculatedZone = Zone.Notification;
			else if (UserDisplayDistance >= STimSettings.NotificationZoneConstrain)
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
