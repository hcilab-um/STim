using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.Globalization;

namespace STimWPF.Util
{

	[Serializable]
	[TypeConverter(typeof(Point3DConverter))]
	public partial struct Point3D : IFormattable
	{

		private double _x;
		private double _y;
		private double _z;

		private bool _isFrozen;
		private double _fx;
		private double _fy;
		private double _fz;

		/// <summary>
		///     X - double.  Default value is 0.
		/// </summary>
		public double X
		{
			get { return _x; }
			set { _x = value; }
		}

		/// <summary>
		///     Y - double.  Default value is 0.
		/// </summary>
		public double Y
		{
			get { return _y; }
			set { _y = value; }
		}

		/// <summary>
		///     Z - double.  Default value is 0.
		/// </summary>
		public double Z
		{
			get { return _z; }
			set { _z = value; }
		}

		public bool IsFrozen
		{
			get { return _isFrozen; }
			set { _isFrozen = value; }
		}

		/// <summary>
		///     X - double.  Default value is 0.
		/// </summary>
		public double FrozenX
		{
			get { return _fx; }
			set { _fx = value; }
		}

		/// <summary>
		///     Y - double.  Default value is 0.
		/// </summary>
		public double FrozenY
		{
			get { return _fy; }
			set { _fy = value; }
		}

		/// <summary>
		///     Z - double.  Default value is 0.
		/// </summary>
		public double FrozenZ
		{
			get { return _fz; }
			set { _fz = value; }
		}

		/// <summary>
		/// Constructor that sets point's initial values.
		/// </summary>
		/// <param name="x">Value of the X coordinate of the new point.
		/// <param name="y">Value of the Y coordinate of the new point.
		/// <param name="z">Value of the Z coordinate of the new point.
		public Point3D(double x, double y, double z)
		{
			_x = x;
			_y = y;
			_z = z;

			_isFrozen = false;
			_fx = 0;
			_fy = 0;
			_fz = 0;
		}

		/// <summary>
		/// Offset - update point position by adding offsetX to X, offsetY to Y, and offsetZ to Z.
		/// </summary>
		/// <param name="offsetX">Offset in the X direction.
		/// <param name="offsetY">Offset in the Y direction.
		/// <param name="offsetZ">Offset in the Z direction.
		public void Offset(double offsetX, double offsetY, double offsetZ)
		{
			_x += offsetX;
			_y += offsetY;
			_z += offsetZ;
		}

		/// <summary>
		/// Point3D + Vector3D addition.
		/// </summary>
		/// <param name="point">Point being added.
		/// <param name="vector">Vector being added.
		/// <returns>Result of addition.</returns>
		public static Point3D operator +(Point3D point, Vector3D vector)
		{
			return new Point3D(point._x + vector.X,
					  point._y + vector.Y,
					  point._z + vector.Z);
		}

		/// <summary>
		/// Point3D + Vector3D addition.
		/// </summary>
		/// <param name="point">Point being added.
		/// <param name="vector">Vector being added.
		/// <returns>Result of addition.</returns>
		public static Point3D Add(Point3D point, Vector3D vector)
		{
			return new Point3D(point._x + vector.X,
					  point._y + vector.Y,
					  point._z + vector.Z);
		}

		/// <summary>
		/// Point3D - Vector3D subtraction.
		/// </summary>
		/// <param name="point">Point from which vector is being subtracted.
		/// <param name="vector">Vector being subtracted from the point.
		/// <returns>Result of subtraction.</returns>
		public static Point3D operator -(Point3D point, Vector3D vector)
		{
			return new Point3D(point._x - vector.X,
					  point._y - vector.Y,
					  point._z - vector.Z);
		}

		/// <summary>
		/// Point3D - Vector3D subtraction.
		/// </summary>
		/// <param name="point">Point from which vector is being subtracted.
		/// <param name="vector">Vector being subtracted from the point.
		/// <returns>Result of subtraction.</returns>
		public static Point3D Subtract(Point3D point, Vector3D vector)
		{
			return new Point3D(point._x - vector.X,
					  point._y - vector.Y,
					  point._z - vector.Z);
		}

		/// <summary>
		/// Subtraction.
		/// </summary>
		/// <param name="point1">Point from which we are subtracting the second point.
		/// <param name="point2">Point being subtracted.
		/// <returns>Vector between the two points.</returns>
		public static Vector3D operator -(Point3D point1, Point3D point2)
		{
			return new Vector3D(point1._x - point2._x,
					  point1._y - point2._y,
					  point1._z - point2._z);
		}

		/// <summary>
		/// Subtraction.
		/// </summary>
		/// <param name="point1">Point from which we are subtracting the second point.
		/// <param name="point2">Point being subtracted.
		/// <returns>Vector between the two points.</returns>
		public static Vector3D Subtract(Point3D point1, Point3D point2)
		{
			Vector3D v = new Vector3D(point1._x - point2._x, point1._y - point2._y, point1._z - point2._z);
			return v;
		}

		/// <summary>
		/// Explicit conversion to Vector3D.
		/// </summary>
		/// <param name="point">Given point.
		/// <returns>Vector representing the point.</returns>
		public static explicit operator Vector3D(Point3D point)
		{
			return new Vector3D(point._x, point._y, point._z);
		}

		/// <summary>
		/// Explicit conversion to Vector3D.
		/// </summary>
		/// <param name="point">Given point.
		/// <returns>Vector representing the point.</returns>
		public static explicit operator System.Windows.Media.Media3D.Point3D(Point3D point)
		{
			return new System.Windows.Media.Media3D.Point3D(point._x, point._y, point._z);
		}

		/// <summary>
		/// Explicit conversion to Point4D.
		/// </summary>
		/// <param name="point">Given point.
		/// <returns>4D point representing the 3D point.</returns>
		public static explicit operator Point4D(Point3D point)
		{
			return new Point4D(point._x, point._y, point._z, 1.0);
		}

		/// <summary>
		/// Compares two Point3D instances for exact equality.
		/// Note that double values can acquire error when operated upon, such that
		/// an exact comparison between two values which are logically equal may fail.
		/// Furthermore, using this equality operator, Double.NaN is not equal to itself.
		/// </summary>
		/// <returns>
		/// bool - true if the two Point3D instances are exactly equal, false otherwise
		/// </returns>
		/// <param name="point1">The first Point3D to compare
		/// <param name="point2">The second Point3D to compare
		public static bool operator ==(Point3D point1, Point3D point2)
		{
			return point1.X == point2.X &&
				   point1.Y == point2.Y &&
				   point1.Z == point2.Z;
		}

		/// <summary>
		/// Compares two Point3D instances for exact inequality.
		/// Note that double values can acquire error when operated upon, such that
		/// an exact comparison between two values which are logically equal may fail.
		/// Furthermore, using this equality operator, Double.NaN is not equal to itself.
		/// </summary>
		/// <returns>
		/// bool - true if the two Point3D instances are exactly unequal, false otherwise
		/// </returns>
		/// <param name="point1">The first Point3D to compare
		/// <param name="point2">The second Point3D to compare
		public static bool operator !=(Point3D point1, Point3D point2)
		{
			return !(point1 == point2);
		}

		/// <summary>
		/// Compares two Point3D instances for object equality.  In this equality
		/// Double.NaN is equal to itself, unlike in numeric equality.
		/// Note that double values can acquire error when operated upon, such that
		/// an exact comparison between two values which
		/// are logically equal may fail.
		/// </summary>
		/// <returns>
		/// bool - true if the two Point3D instances are exactly equal, false otherwise
		/// </returns>
		/// <param name="point1">The first Point3D to compare
		/// <param name="point2">The second Point3D to compare
		public static bool Equals(Point3D point1, Point3D point2)
		{
			return point1.X.Equals(point2.X) &&
				   point1.Y.Equals(point2.Y) &&
				   point1.Z.Equals(point2.Z);
		}

		/// <summary>
		/// Equals - compares this Point3D with the passed in object.  In this equality
		/// Double.NaN is equal to itself, unlike in numeric equality.
		/// Note that double values can acquire error when operated upon, such that
		/// an exact comparison between two values which
		/// are logically equal may fail.
		/// </summary>
		/// <returns>
		/// bool - true if the object is an instance of Point3D and if it's equal to "this".
		/// </returns>
		/// <param name="o">The object to compare to "this"
		public override bool Equals(object o)
		{
			if ((null == o) || !(o is Point3D))
			{
				return false;
			}

			Point3D value = (Point3D)o;
			return Point3D.Equals(this, value);
		}

		/// <summary>
		/// Equals - compares this Point3D with the passed in object.  In this equality
		/// Double.NaN is equal to itself, unlike in numeric equality.
		/// Note that double values can acquire error when operated upon, such that
		/// an exact comparison between two values which
		/// are logically equal may fail.
		/// </summary>
		/// <returns>
		/// bool - true if "value" is equal to "this".
		/// </returns>
		/// <param name="value">The Point3D to compare to "this"
		public bool Equals(Point3D value)
		{
			return Point3D.Equals(this, value);
		}

		/// <summary>
		/// Returns the HashCode for this Point3D
		/// </summary>
		/// <returns>
		/// int - the HashCode for this Point3D
		/// </returns>
		public override int GetHashCode()
		{
			// Perform field-by-field XOR of HashCodes
			return X.GetHashCode() ^
				   Y.GetHashCode() ^
				   Z.GetHashCode();
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return String.Format("X = {0} | Y = {1} | Z = {2}", _x, _y, _z);
		}
	}
}
