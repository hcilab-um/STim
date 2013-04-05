﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace KinectWPF3D
{
	public class Math3D
	{
		public static Matrix3D SetViewMatrix(Point3D cameraPosition, Vector3D lookDirection, Vector3D upDirection)
		{
			// Normalize vectors:
			lookDirection.Normalize();
			upDirection.Normalize();
			// Define vectors, XScale, YScale, and ZScale:
			double denom = Math.Sqrt(1 - Math.Pow(Vector3D.DotProduct(lookDirection, upDirection), 2));
			Vector3D XScale = Vector3D.CrossProduct(lookDirection, upDirection) / denom;
			Vector3D YScale = (upDirection - Vector3D.DotProduct(upDirection, lookDirection) * lookDirection) / denom;
			Vector3D ZScale = lookDirection;

			// Construct M matrix:
			Matrix3D M = new Matrix3D()
			{
				M11 = XScale.X,
				M12 = YScale.X,
				M13 = ZScale.X,
				M21 = XScale.Y,
				M22 = YScale.Y,
				M23 = ZScale.Y,
				M31 = XScale.Z,
				M32 = YScale.Z,
				M33 = ZScale.Z
			};

			// Translate the camera position to the origin:
			Matrix3D translateMatrix = new Matrix3D();
			translateMatrix.Translate(new Vector3D(-cameraPosition.X, -cameraPosition.Y, -cameraPosition.Z));
			// Define reflect matrix about the Z axis:
			Matrix3D reflectMatrix = new Matrix3D();
			reflectMatrix.M33 = -1;
			// Construct the View matrix:
			Matrix3D viewMatrix =
			translateMatrix * M * reflectMatrix;
			return viewMatrix;
		}

		public static Matrix3D SetPerspectiveOffCenter(double left, double right, double bottom, double top, double near, double far)
		{
			Matrix3D perspectiveMatrix = new Matrix3D();
			perspectiveMatrix.M11 = 2 * near / (right - left);
			perspectiveMatrix.M22 = 2 * near / (top - bottom);
			perspectiveMatrix.M31 = (right + left) / (right - left);
			perspectiveMatrix.M32 = (top + bottom) / (top - bottom);
			perspectiveMatrix.M33 = far / (near - far);
			perspectiveMatrix.M34 = -1.0;
			perspectiveMatrix.OffsetZ = near * far / (near - far);
			perspectiveMatrix.M44 = 0;
			return perspectiveMatrix;
		}

		public static Matrix3D SetPerspective(double width, double height, double near, double far)
		{
			Matrix3D perspectiveMatrix = new Matrix3D();
			perspectiveMatrix.M11 = 2 * near / width;
			perspectiveMatrix.M22 = 2 * near / height;
			perspectiveMatrix.M33 = far / (near - far);
			perspectiveMatrix.M34 = -1.0;
			perspectiveMatrix.OffsetZ = near * far / (near - far);
			perspectiveMatrix.M44 = 0;
			return perspectiveMatrix;
		}

		public static Matrix3D SetPerspectiveFov(double fov, double aspectRatio, double near, double far)
		{
			Matrix3D perspectiveMatrix = new Matrix3D();
			double yscale = 1.0 / Math.Tan(fov * Math.PI / 180 / 2);
			double xscale = yscale / aspectRatio;
			perspectiveMatrix.M11 = xscale;
			perspectiveMatrix.M22 = yscale;
			perspectiveMatrix.M33 = far / (near - far);
			perspectiveMatrix.M34 = -1.0;
			perspectiveMatrix.OffsetZ = near * far / (near - far);
			perspectiveMatrix.M44 = 0;
			return perspectiveMatrix;
		}
	}
}
