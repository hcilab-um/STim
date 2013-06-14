using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpikeWPF
{
	public class AttentionEstimator
	{
		private const double ORIENTATION_PARAMETER = 0.5;
		private const double BOTHER_PARAMETER = 6.0;
		private const double TRACKING_DISTANCE = 5.0;
	
		private double orientationAngleAlpha;
		private double botherAngleBeta;
		private double userDisplayDistance;

		private double CalculateBotherEffect()
		{
			return Math.Tanh((botherAngleBeta - 45) * BOTHER_PARAMETER * Math.PI / 180 / 2) + 0.5;
		}

		private double CalculateOrientationEffect()
		{
			return 1 - Math.Pow(orientationAngleAlpha / 180, 0.5);
		}

		private double CalculateDistanceEffect()
		{
			return TRACKING_DISTANCE / userDisplayDistance;
		}

	}
}
