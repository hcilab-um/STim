using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.STim.Nodes.Attention
{
	public struct AttentionSocial
	{
		public double SocialEffect { get; set; }
		public double DistanceEffect { get; set; }
		public double OrientationEffect { get; set; }
		public double SocialAttentionValue { get; set; }

		public override string ToString()
		{
			String output = String.Format("So: {0}", SocialAttentionValue.ToString(".00"));
			//String.Format("Social({0},{1},{2}) = {3}", OrientationEffect.ToString(".00"), DistanceEffect.ToString(".00"), SocialEffect.ToString(".00"), SocialAttentionValue.ToString(".00"));
			return output;
		}			
	}
}
