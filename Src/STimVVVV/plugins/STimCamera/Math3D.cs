using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace VVVV.Nodes
{
	public class Math3D
	{
		/// <summary>
		/// LookAtRH
		/// </summary>
		/// http://msdn.microsoft.com/en-us/library/bb281711(v=vs.85).aspx
		/// <returns></returns>
		public static VVVV.Utils.VMath.Matrix4x4 SetViewMatrix(Vector3D cameraPosition, Vector3D lookDirection, Vector3D upDirection)
		{
			// Normalize vectors:
			lookDirection.Normalize();
			upDirection.Normalize();
			double dotProduct = Vector3D.DotProduct(lookDirection, upDirection);
			// Define vectors, XScale, YScale, and ZScale:
			double denom = Math.Sqrt(1 - Math.Pow(dotProduct, 2));
			Vector3D XScale = Vector3D.CrossProduct(lookDirection, upDirection) / denom;
			Vector3D YScale = (upDirection - dotProduct * lookDirection) / denom;
			Vector3D ZScale = lookDirection;

			// Construct M matrix:
			Matrix3D M = new Matrix3D()
			{
				M11 = XScale.X, M12 = YScale.X, M13 = ZScale.X,
				M21 = XScale.Y, M22 = YScale.Y, M23 = ZScale.Y,
				M31 = XScale.Z, M32 = YScale.Z, M33 = ZScale.Z
			};

			// Translate the camera position to the origin:
			Matrix3D translateMatrix = new Matrix3D();
			translateMatrix.Translate(new Vector3D(-cameraPosition.X, -cameraPosition.Y, -cameraPosition.Z));
			// Define reflect matrix about the Z axis:
			Matrix3D reflectMatrix = new Matrix3D();
			reflectMatrix.M33 = -1;
		
			// Construct the View matrix:
			Matrix3D viewMatrix = translateMatrix * M * reflectMatrix;
			return ConvertMatrix4x4(viewMatrix);
		}

		/// <summary>
		/// PerspectiveOffCenterRH
		/// </summary>
		/// http://msdn.microsoft.com/en-us/library/bb281731(v=vs.85).aspx
		/// <returns></returns>
		public static VVVV.Utils.VMath.Matrix4x4 SetPerspectiveOffCenter(double left, double right, double bottom, double top, double near, double far)
		{
			//Right H
			Matrix3D perspectiveMatrix = new Matrix3D();
			perspectiveMatrix.M11 = 2 * near / (right - left);
			perspectiveMatrix.M22 = 2 * near / (top - bottom);
			perspectiveMatrix.M31 = (right + left) / (right - left);
			perspectiveMatrix.M32 = (top + bottom) / (top - bottom);
			perspectiveMatrix.M33 = far / (near - far);
			perspectiveMatrix.M34 = -1.0;
			perspectiveMatrix.OffsetZ = near * far / (near - far);
			perspectiveMatrix.M44 = 0;
			return ConvertMatrix4x4(perspectiveMatrix);
			//return perspectiveMatrix;
		}

		private static VVVV.Utils.VMath.Matrix4x4 ConvertMatrix4x4(Matrix3D inputMatrix)
		{
			VVVV.Utils.VMath.Matrix4x4 resultMatrix = new Utils.VMath.Matrix4x4();
			resultMatrix.m11 = inputMatrix.M11;
			resultMatrix.m12 = inputMatrix.M12;
			resultMatrix.m13 = inputMatrix.M13;
			resultMatrix.m14 = inputMatrix.M14;

			resultMatrix.m21 = inputMatrix.M21;
			resultMatrix.m22 = inputMatrix.M22;
			resultMatrix.m23 = inputMatrix.M23;
			resultMatrix.m24 = inputMatrix.M24;

			resultMatrix.m31 = inputMatrix.M31;
			resultMatrix.m32 = inputMatrix.M32;
			resultMatrix.m33 = inputMatrix.M33;
			resultMatrix.m34 = inputMatrix.M34;

			resultMatrix.m41 = inputMatrix.OffsetX;
			resultMatrix.m42 = inputMatrix.OffsetY;
			resultMatrix.m43 = inputMatrix.OffsetZ;
			resultMatrix.m44 = inputMatrix.M44;
			return resultMatrix;
		}
	}
}
