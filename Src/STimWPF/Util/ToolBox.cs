using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace STimWPF.Util
{
	public class ToolBox
	{
		internal static Vector3D CalculateDisplacement(Vector3D lastPos, Vector3D currentPos)
		{
			Vector3D displacement = new Vector3D(0, 0, 0);
			if (lastPos.Length == 0)
				return displacement;

			displacement.X = currentPos.X - lastPos.X;
			displacement.Y = currentPos.Y - lastPos.Y;
			displacement.Z = currentPos.Z - lastPos.Z;
			return displacement;
		}
	}
}
