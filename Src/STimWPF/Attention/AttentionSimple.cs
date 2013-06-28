using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STimWPF.Attention
{
	public struct AttentionSimple
	{
		public double SimpleDistanceEffect { get; set; }
		public double SimpleOrientationEffect { get; set; }
		public double SimpleAttentionValue { get; set; }

		public override string ToString()
		{
			return String.Format("Simple({0},{1}) = {2}", SimpleOrientationEffect.ToString(".00"), SimpleDistanceEffect.ToString(".00"), SimpleAttentionValue.ToString(".00"));
		}
	}
}
