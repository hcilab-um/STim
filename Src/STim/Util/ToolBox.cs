using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Xna.Framework;

namespace STim.Util
{
	public static class ToolBox
	{
		public static Matrix3D ToMatrix3D(this Matrix matrix)
		{
			Matrix3D matrix3D = new Matrix3D()
			{
				M11 = matrix.M11,
				M12 = matrix.M12,
				M13 = matrix.M13,
				M14 = matrix.M14,

				M21 = matrix.M21,
				M22 = matrix.M22,
				M23 = matrix.M23,
				M24 = matrix.M24,

				M31 = matrix.M31,
				M32 = matrix.M32,
				M33 = matrix.M33,
				M34 = matrix.M34,

				OffsetX = matrix.M41,
				OffsetY = matrix.M42,
				OffsetZ = matrix.M43,
				M44 = matrix.M44
			};

			return matrix3D;
		}

		public static Vector3 ToVector3(this Vector3D vector3D)
		{
			return new Vector3() { X = (float)vector3D.X, Y = (float)vector3D.Y, Z = (float)vector3D.Z };
		}

		public static Point3D ToPoint3D(this SkeletonPoint skelPoint)
		{
			return new Point3D() { X = skelPoint.X, Y = skelPoint.Y, Z = skelPoint.Z };
		}

		public static Vector3D ToVector3D(this Point3D point)
		{
			return new Vector3D() { X = point.X, Y = point.Y, Z = point.Z };
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