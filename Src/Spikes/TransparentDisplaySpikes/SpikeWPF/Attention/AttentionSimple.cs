﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpikeWPF.Attention
{
	public struct AttentionSimple
	{
		public double SimpleDistanceEffect;
		public double SimpleOrientationEffect;
		public double SimpleAttentionValue;

		public override string ToString()
		{
			return String.Format("Simple({0},{1}) = {2}", SimpleOrientationEffect.ToString(".00"), SimpleDistanceEffect.ToString(".00"), SimpleAttentionValue.ToString(".00"));
		}
	}
}
