using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace STimAttentionWPF.Graphic3D
{
	/// <summary>
	/// Resource from: http://www.codeproject.com/Articles/14414/Generating-a-sphere-mesh-in-XAML
	/// </summary>
	public abstract class RoundMesh3D
	{
		protected int n = 10;
		protected double r = 20;
		protected Point3DCollection points;
		protected Int32Collection triangleIndices;

		public virtual double Radius
		{
			get { return r; }
			set { r = value; CalculateGeometry(); }
		}

		public virtual int Separators
		{
			get { return n; }
			set { n = value; CalculateGeometry(); }
		}

		public Point3DCollection Points
		{
			get { return points; }
		}

		public Int32Collection TriangleIndices
		{
			get { return triangleIndices; }
		}

		protected abstract void CalculateGeometry();
	}
}
