using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STimWPF.Util
{
	public class DepthPercentFilter
	{
		private CircularList<double> PercentageBuffer { get; set; }
		
		public DepthPercentFilter(int bufferSize)
		{
			PercentageBuffer = new CircularList<double>(bufferSize);
		}

		public double ProcessNewPercentageData(double newPercentage)
		{
			PercentageBuffer.Value = newPercentage;
			PercentageBuffer.Next();
			return PercentageBuffer.Sum() / PercentageBuffer.Count;
		}
	}
}
