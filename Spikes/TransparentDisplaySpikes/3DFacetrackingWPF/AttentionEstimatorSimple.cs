using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;

namespace SpikeWPF
{
	public class AttentionEstimatorSimple
	{
		private const double ORIENTATION_PARAMETER = 0.5;
		private const double DISTANCE_PARAMETER = 0.5;

		private const double TRACKING_DISTANCE = 5.0;
		private const double VISUAL_FIELD = 120;

		public AttentionSimple CalculateAttention(Skeleton userSkeleton)
		{
			double orientationEffect = 0;
			double orientationAngle = CalculateOrientationAngle(userSkeleton);
			double orientationAngleAbs = Math.Abs(orientationAngle);
			if (orientationAngleAbs <= VISUAL_FIELD / 2)
				orientationEffect = (1 - orientationAngleAbs / (VISUAL_FIELD / 2)) * 100;

			double distanceEffect = (1 - userSkeleton.Position.Z / TRACKING_DISTANCE) * 100;

			double attention = orientationEffect * ORIENTATION_PARAMETER + distanceEffect * DISTANCE_PARAMETER;
			
			AttentionSimple attentionSimple = new AttentionSimple()
			{
				SimpleDistanceEffect = distanceEffect,
				SimpleOrientationEffect = orientationEffect,
				SimpleAttentionValue = attention
			};

			return attentionSimple;
		}

		//public double CalculateBotherAngleBeta(Skeleton userSkeleton, Skeleton botherSkeleton)
		//{
		//  Point userPoint = new Point(userSkeleton.Position.X, userSkeleton.Position.Z);

		//  Point botherPoint = new Point(botherSkeleton.Position.X, botherSkeleton.Position.Z);

		//  Vector userBotherVector = new Vector(botherPoint.X - userPoint.X, botherPoint.Y - userPoint.Y);
		//  Vector userDisplayVector = -(Vector)userPoint;
			
		//  double botherAngleBeta = Math.Abs(Vector.AngleBetween(userBotherVector, userDisplayVector));
		//  return botherAngleBeta;
		//}

		public double CalculateOrientationAngle(Skeleton userSkeleton)
		{
			Joint shoulderLeft = userSkeleton.Joints.SingleOrDefault(temp => temp.JointType == JointType.ShoulderLeft);
			Joint shoulderRight = userSkeleton.Joints.SingleOrDefault(temp => temp.JointType == JointType.ShoulderRight);
			
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
