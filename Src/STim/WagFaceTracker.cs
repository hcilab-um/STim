using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Microsoft.Kinect;

namespace STim
{
	public class WagFaceTracker
	{
		public bool IsUsing { get; set; }
		public int SkeletonId { get; set; }

		public FaceTracker FaceTracker { get; set; }

		public WagFaceTracker(KinectSensor kinectSensor)
		{
			FaceTracker = new FaceTracker(kinectSensor);
			IsUsing = false;
			SkeletonId = -1;
		}
	}
}
