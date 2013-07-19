using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;

namespace STim.Attention
{
	public class AttentionEstimatorSimple
	{
		private const double ORIENTATION_PARAMETER = 0.5;
		private const double DISTANCE_PARAMETER = 0.5;

		private const double TRACKING_DISTANCE = 5.0;
		private const double VISUAL_FIELD = 120;

		public AttentionSimple CalculateAttention(WagSkeleton userSkeleton)
		{
			double orientationEffect = 0;
			if (userSkeleton.BodyOrientationAngle <= VISUAL_FIELD / 2)
				orientationEffect = (1 - userSkeleton.BodyOrientationAngle / (VISUAL_FIELD / 2)) * 100;

			double distanceEffect = (1 - userSkeleton.TransformedPosition.Z / TRACKING_DISTANCE) * 100;

			double attention = orientationEffect * ORIENTATION_PARAMETER + distanceEffect * DISTANCE_PARAMETER;
			
			AttentionSimple attentionSimple = new AttentionSimple()
			{
				SimpleDistanceEffect = distanceEffect,
				SimpleOrientationEffect = orientationEffect,
				SimpleAttentionValue = attention
			};

			return attentionSimple;
		}

		private double CalculateOrientationAngle(WagSkeleton userSkeleton)
		{
			Joint shoulderLeft = userSkeleton.TransformedJoints.SingleOrDefault(temp => temp.JointType == JointType.ShoulderLeft);
			Joint shoulderRight = userSkeleton.TransformedJoints.SingleOrDefault(temp => temp.JointType == JointType.ShoulderRight);
			
			Point shoulderRightP = new Point(shoulderRight.Position.X, shoulderRight.Position.Z);
			Point shoulderLeftP = new Point(shoulderLeft.Position.X, shoulderLeft.Position.Z);
			
			Vector shoulderVector = new Vector(shoulderRightP.X - shoulderLeftP.X, shoulderRightP.Y - shoulderLeftP.Y);
			Vector xAxis = new Vector (1, 0);

			double orientationAngle = Vector.AngleBetween(xAxis, shoulderVector);
			
			if(orientationAngle < 0)
				orientationAngle = - orientationAngle;

			return orientationAngle;
		}

	}
}
