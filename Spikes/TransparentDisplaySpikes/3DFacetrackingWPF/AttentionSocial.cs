using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpikeWPF
{
	public struct AttentionSocial
	{
		public double SocialEffect;
		public double DistanceEffect;
		public double OrientationEffect;
		public double AttentionValue;

		public override string ToString()
		{
			return "Social(" + OrientationEffect.ToString(".00") + ", " + DistanceEffect.ToString(".00") + ", " + SocialEffect.ToString(".00") + ") = " + AttentionValue.ToString(".00");
		}
	}
}
