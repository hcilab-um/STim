using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Windows.Media.Media3D;
using System.ComponentModel;

namespace STim
{

	public class WagSkeleton : Skeleton, INotifyPropertyChanged
	{
		public FaceTrackFrame FaceFrame { get; set; }
		
		public Attention.AttentionSocial AttentionSocial { get; set; }
		public Attention.AttentionSimple AttentionSimple { get; set; }

		public WagJointCollection TransformedJoints;

		public double BodyOrientationAngle { get; set; }

		public Point3D TransformedPosition { get; set; }

		private Point3D headLocation = new Point3D(0, 0, 3);
		private Vector3D headOrientation = new Vector3D(0, 0, 0);

		public Point3D HeadLocation
		{
			get { return headLocation; }
			set
			{
				headLocation= value;
				OnPropertyChanged("HeadLocation");
			}
		}

		public Vector3D HeadOrientation 
		{
			get { return headOrientation; }
			set
			{
				headOrientation = value;
				OnPropertyChanged("HeadOrientation");
			}
		}

		public int LastFrameSeen { get; set; }

		public WagSkeleton(Skeleton source)
		{
			TrackingId = source.TrackingId;
			Update(source);
		}

		public void Update(Skeleton source)
		{
			BoneOrientations = source.BoneOrientations;
			ClippedEdges = source.ClippedEdges;
			Joints = source.Joints;
			Position = source.Position;
			TrackingState = source.TrackingState;
			TransformedJoints = new WagJointCollection(Joints);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}

}
