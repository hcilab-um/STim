﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STimWPF.Util;

namespace STimWPF
{
	public class DepthPercentFilter
	{
		private CircularList<int> PercentageBuffer { get; set; }
		
		public DepthPercentFilter(int bufferSize)
		{
			PercentageBuffer = new CircularList<int>(bufferSize);
		}

		public int ProcessNewPercentageData(int newPercentage)
		{
			PercentageBuffer.Value = newPercentage;
			PercentageBuffer.Next();
			return PercentageBuffer.Sum() / PercentageBuffer.Count;
		}
	}
}