using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;

namespace SpikeWPF
{
	public class AttentionEstimator
	{
		private const double ORIENTATION_PARAMETER = 0.5;
		private const double BOTHER_PARAMETER = 6.0;
		private const double TRACKING_DISTANCE = 5.0;

		private double CalculateBotherAngleBeta(Skeleton userSkeleton, Skeleton botherSkeleton)
		{
			Vector userVector = new Vector(userSkeleton.Position.X, userSkeleton.Position.Z);
			Vector botherVector = new Vector(botherSkeleton.Position.X, botherSkeleton.Position.Z);
			return Vector.AngleBetween(userVector, botherVector);
		}

		private double CalculateOrientationAngle(Skeleton userSkeleton)
		{
			Joint shoulderLeft = userSkeleton.Joints.SingleOrDefault(temp => temp.JointType == JointType.ShoulderLeft);
			Joint shoulderRight = userSkeleton.Joints.SingleOrDefault(temp => temp.JointType == JointType.ShoulderRight);
			
			Point shoulderRightP = new Point(shoulderRight.Position.X, shoulderRight.Position.Z);
			Point shoulderLeftP = new Point(shoulderLeft.Position.X, shoulderLeft.Position.Z);
			
			Vector shoulderVector = new Vector(shoulderRightP.X - shoulderLeftP.X, shoulderRightP.Y - shoulderLeftP.Y);
			Vector xAxis = new Vector (1, 0);
			double shoulderAngle = Vector.AngleBetween(xAxis, shoulderVector);
			
			if(shoulderAngle >90)
				shoulderAngle = 180 - shoulderAngle;

			double orientationAngle = 90 - shoulderAngle;
			
			return orientationAngle;
		}

		public double CalculateAttention(Skeleton userSkeleton, List<Skeleton> skeletons)
		{
			double botherAngleBeta = 0;
			double orientationAngleAlpha = 0;
			
			//Calculate Social Function: Bother effect
			double minBotherEffect = 0;
			skeletons.Remove(userSkeleton);
			List<double> botherList = new List<double>();
			foreach (Skeleton skel in skeletons)
			{
				botherAngleBeta = CalculateBotherAngleBeta(userSkeleton, skel);
				double botherEffect = Math.Tanh((botherAngleBeta - 45) * BOTHER_PARAMETER * Math.PI / 180 / 2) + 0.5;
				botherList.Add(botherEffect);
			}

			if(botherList.Count != 0)
				minBotherEffect = botherList.Min();
			
			orientationAngleAlpha = CalculateOrientationAngle(userSkeleton);
			double orientationEffect = 1 - Math.Pow(orientationAngleAlpha / 180, 0.5);

			double distanceEffect = TRACKING_DISTANCE / userSkeleton.Position.Z;

			double attention = orientationEffect * distanceEffect * minBotherEffect;
			return attention;

		}
	}
}
