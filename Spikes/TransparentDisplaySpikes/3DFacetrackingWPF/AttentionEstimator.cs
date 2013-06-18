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
		
		public double CalculateBotherAngleBeta(Skeleton userSkeleton, Skeleton botherSkeleton)
		{
			Point userPoint = new Point(userSkeleton.Position.X, userSkeleton.Position.Z);

			Point botherPoint = new Point(botherSkeleton.Position.X, botherSkeleton.Position.Z);

			//Point userPoint = new Point(-5, 20);
			//Point botherPoint = new Point(0, 1);
			Vector userBotherVector = new Vector(botherPoint.X - userPoint.X, botherPoint.Y - userPoint.Y);
			Vector userDisplayVector = -(Vector)userPoint;
			
			double botherAngleBeta = Math.Abs(Vector.AngleBetween(userBotherVector, userDisplayVector));
			return botherAngleBeta;
		}

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

		public double CalculateSocialEffect(Skeleton userSkeleton, List<Skeleton> skeletons)
		{
			double botherAngleBeta = 180;
			double minBotherEffect = 0;
			List<double> botherList = new List<double>();
			foreach (Skeleton skel in skeletons)
			{
				if (skel == userSkeleton)
					continue;
				botherAngleBeta = CalculateBotherAngleBeta(userSkeleton, skel);
				double botherEffect = Math.Tanh((botherAngleBeta - 45) * BOTHER_PARAMETER * Math.PI / 180 / 2) + 0.5;
				botherList.Add(botherEffect);
				//botherList.Add(botherAngleBeta);
			}

			if (botherList.Count != 0)
				minBotherEffect = botherList.Min();
			return minBotherEffect;
		}

		public double CalculateAttention(Skeleton userSkeleton, List<Skeleton> skeletons)
		{
			double orientationAngleAlpha = 0;
			
			//Calculate Social Function: Bother effect
			double socialEffect = CalculateSocialEffect(userSkeleton, skeletons);
			
			orientationAngleAlpha = CalculateOrientationAngle(userSkeleton);
			double orientationEffect = 1 - Math.Pow(orientationAngleAlpha / 180, 0.5);

			double distanceEffect = TRACKING_DISTANCE / userSkeleton.Position.Z;

			double attention = orientationEffect * distanceEffect * socialEffect;
			return attention;
		}

	}
}
