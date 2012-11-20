using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STimWPF.Util;
using Microsoft.Kinect;

namespace STimWPF
{

	public class SkeletonFilter
	{

		public Skeleton StableSkeleton { get; set; }

		private CircularList<Skeleton> SkeletonsBuffer { get; set; }

		public SkeletonFilter(int bufferSize)
		{
			SkeletonsBuffer = new CircularList<Skeleton>(bufferSize);
		}

		public Skeleton ProcessNewSkeletonData(Skeleton newData)
		{
			//Process it and updates the StableSkeleton
			SkeletonsBuffer.Value = newData;
			SkeletonsBuffer.Next();

			//Calculates the average skeleton from all those in the circular list
			Skeleton stableSkeleton = new Skeleton();
			stableSkeleton.TrackingState = SkeletonTrackingState.Tracked;
			stableSkeleton.ClippedEdges = newData.ClippedEdges;
			stableSkeleton.TrackingId = newData.TrackingId;

			foreach (JointType joint in Enum.GetValues(typeof(JointType)))
			{
				Joint avgJoint = stableSkeleton.Joints[joint];
				avgJoint.TrackingState = JointTrackingState.Tracked;
				avgJoint.Position = GetAvgPosition(joint);

				stableSkeleton.Joints[joint] = avgJoint;
			}

			StableSkeleton = stableSkeleton;
			return StableSkeleton;
		}

		private SkeletonPoint GetAvgPosition(JointType joint)
		{
			float avgX = 0, avgY = 0, avgZ = 0;

			avgX = SkeletonsBuffer.Sum(skeleton => skeleton.Joints[joint].Position.X) / SkeletonsBuffer.Count;
			avgY = SkeletonsBuffer.Sum(skeleton => skeleton.Joints[joint].Position.Y) / SkeletonsBuffer.Count;
			avgZ = SkeletonsBuffer.Sum(skeleton => skeleton.Joints[joint].Position.Z) / SkeletonsBuffer.Count;

			return new SkeletonPoint() { X = avgX, Y = avgY, Z = avgZ };
		}


		public void Reset()
		{
			if (SkeletonsBuffer.Count == 0)
				return;
			SkeletonsBuffer = new CircularList<Skeleton>(SkeletonsBuffer.Count);
		}
	}

}
