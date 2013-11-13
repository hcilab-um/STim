using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace SpikeWPF
{
	public static class Util
	{
		public static Point3D ToPoint3D(this SkeletonPoint skelPoint)
		{
			return new Point3D() { X = skelPoint.X, Y = skelPoint.Y, Z = skelPoint.Z };
		}
	}
}
