using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.STim.Nodes.Util
{
	public class DepthPercentFilter
	{
		private CircularList<double> PercentageBuffer { get; set; }

		public DepthPercentFilter()
		{
			PercentageBuffer = new CircularList<double>(1);
		}

		public double ProcessNewPercentageData(double newPercentage, int bufferSize)
		{
			if (PercentageBuffer == null || PercentageBuffer.Length != bufferSize)
			{
				CircularList<double> newList = new CircularList<double>(bufferSize);
				
				foreach (double value in PercentageBuffer)
				{
					newList.Value = value;
					newList.Next();
				}

				PercentageBuffer = newList;
			}

			PercentageBuffer.Value = newPercentage;
			PercentageBuffer.Next();
			return PercentageBuffer.Sum() / PercentageBuffer.Count;
		}
	}
}
