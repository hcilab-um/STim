﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SpikeWPF.Attention
{
	public class AttentionEstimatorSocial
	{
		private const double ORIENTATION_PARAMETER = 0.5;
		private const double BOTHER_PARAMETER = 6.0;
		private const double TRACKING_DISTANCE = 5.0;

		public AttentionSocial CalculateAttention(WagSkeleton userSkeleton, WagSkeleton [] skeletons)
		{
			//Calculate Social Function: Bother effect
			double socialEffect = CalculateSocialEffect(userSkeleton, skeletons);

			double orientationAngleAlpha = CalculateOrientationAngle(userSkeleton);
			double orientationEffect = 1 - Math.Pow(orientationAngleAlpha / 180, 0.5);

			double distanceEffect = TRACKING_DISTANCE / userSkeleton.TransformedPosition.Z;

			double attention = orientationEffect * distanceEffect * socialEffect;
			AttentionSocial attentionSocial = new AttentionSocial()
			{
				DistanceEffect = distanceEffect,
				SocialEffect = socialEffect,
				OrientationEffect = orientationEffect,
				AttentionValue = attention
			};
			return attentionSocial;
		}

		private double CalculateSocialEffect(WagSkeleton userSkeleton, WagSkeleton[] skeletons)
		{
			double minBotherEffect = Math.Tanh((180 - 45) * BOTHER_PARAMETER * Math.PI / 180 / 2) + 0.5;
			List<double> botherList = new List<double>();
			foreach (WagSkeleton skel in skeletons)
			{
				if (skel == userSkeleton)
					continue;
				double botherAngleBeta = CalculateBotherAngleBeta(userSkeleton, skel);
				double botherEffect = Math.Tanh((botherAngleBeta - 45) * BOTHER_PARAMETER * Math.PI / 180 / 2) + 0.5;
				botherList.Add(botherEffect);
			}

			if (botherList.Count > 0)
				minBotherEffect = botherList.Min();
			return minBotherEffect;
		}

		private double CalculateBotherAngleBeta(WagSkeleton userSkeleton, WagSkeleton botherSkeleton)
		{
			Point userPoint = new Point(userSkeleton.TransformedPosition.X, userSkeleton.TransformedPosition.Z);

			Point botherPoint = new Point(botherSkeleton.TransformedPosition.X, botherSkeleton.TransformedPosition.Z);

			//Point userPoint = new Point(-5, 20);
			//Point botherPoint = new Point(0, 1);
			Vector userBotherVector = new Vector(botherPoint.X - userPoint.X, botherPoint.Y - userPoint.Y);
			Vector userDisplayVector = -(Vector)userPoint;
			
			double botherAngleBeta = Math.Abs(Vector.AngleBetween(userBotherVector, userDisplayVector));
			return botherAngleBeta;
		}

		private double CalculateOrientationAngle(WagSkeleton userSkeleton)
		{
			Microsoft.Kinect.Joint shoulderLeft = userSkeleton.TransformedJoints.SingleOrDefault(temp => temp.JointType == Microsoft.Kinect.JointType.ShoulderLeft);
			Microsoft.Kinect.Joint shoulderRight = userSkeleton.TransformedJoints.SingleOrDefault(temp => temp.JointType == Microsoft.Kinect.JointType.ShoulderRight);
			
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
