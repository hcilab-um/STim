using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media.Media3D;

namespace VVVV.STim.Nodes.Util
{
	public static class ToolBox
	{

		public static Point3D ToPoint3D(this SkeletonPoint skelPoint)
		{
			return new Point3D() { X = skelPoint.X, Y = skelPoint.Y, Z = skelPoint.Z };
		}

		internal static Vector GetMovementVector(Point3D lastPos, Point3D currentPos)
		{
			Vector displacement = new Vector(0, 0);
			displacement.X = currentPos.X - lastPos.X;
			displacement.Y = currentPos.Y - lastPos.Y;
			if (Math.Abs(displacement.X) < 0.0001)
				displacement.X = 0;
			if (Math.Abs(displacement.Y) < 0.0001)
				displacement.Y = 0;
			return displacement;
		}

		internal static Vector3D GetMiddleVector(Vector3D P1, Vector3D P2)
		{
			Vector3D middleP = new Vector3D();
			middleP.X = (P1.X + P2.X) / 2;
			middleP.Y = (P1.Y + P2.Y) / 2;
			middleP.Z = (P1.Z + P2.Z) / 2;
			return middleP;
		}

		internal static Matrix3D NewCoordinateMatrix(Vector3D vX, Vector3D vY, Vector3D vZ)
		{
			var matrix = new Matrix3D();
			matrix.M11 = vX.X;
			matrix.M12 = vY.X;
			matrix.M13 = vZ.X;

			matrix.M21 = vX.Y;
			matrix.M22 = vY.Y;
			matrix.M23 = vZ.Y;
			
			matrix.M31 = vX.Z;
			matrix.M32 = vY.Z;
			matrix.M33 = vZ.Z;
			return matrix;
		}

		internal static double AngleToRadian(double angle)
		{
			return angle * Math.PI / 180;
		}
	}
}