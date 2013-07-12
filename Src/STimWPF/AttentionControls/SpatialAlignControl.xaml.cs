using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using _3DTools;
using System.Windows.Media.Media3D;

namespace STimWPF.AttentionControls
{
	/// <summary>
	/// Interaction logic for SpatialAlignControl.xaml
	/// </summary>
	public partial class SpatialAlignControl : UserControl
	{
		private static readonly Point3D startV = new Point3D(-0.530, -0.29, -0.5);
		private static readonly Point3D endV = new Point3D(-0.530, 0.29, -0.5);
		private static readonly Point3D startH = new Point3D(-0.530, -0.29, -0.5);
		private static readonly Point3D endH = new Point3D(0.530, -0.29, -0.5);

		public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register("Distance", typeof(double), typeof(SpatialAlignControl));
		public static readonly DependencyProperty HeadLocationProperty = DependencyProperty.Register("HeadLocation", typeof(Point3D), typeof(SpatialAlignControl));

		public double Distance
		{
			get { return (double)GetValue(DistanceProperty); }
			set { SetValue(DistanceProperty, value); }
		}

		public Point3D HeadLocation
		{
			get { return (Point3D)GetValue(HeadLocationProperty); }
			set { SetValue(HeadLocationProperty, value); }
		}

		public SpatialAlignControl()
		{
			InitializeComponent();
			drawGrid();
		}

		private void drawGrid()
		{
			ScreenSpaceLines3D normal0Wire = new ScreenSpaceLines3D();
			int width = 2;
			normal0Wire.Thickness = width;
			normal0Wire.Color = Colors.Red;

			normal0Wire.Points.Add(startV);
			normal0Wire.Points.Add(endV);

			normal0Wire.Points.Add(new Point3D(0.530, -0.29, -0.5));
			normal0Wire.Points.Add(new Point3D(0.530, 0.29, -0.5));

			normal0Wire.Points.Add(new Point3D(0.530, 0.29, -0.5));
			normal0Wire.Points.Add(new Point3D(-0.530, 0.29, -0.5));

			normal0Wire.Points.Add(startH);
			normal0Wire.Points.Add(endH);

			normal0Wire.Points.Add(new Point3D(0, 0, 0));
			normal0Wire.Points.Add(new Point3D(0, 0, -0.5));

			normal0Wire.Points.Add(new Point3D(0, 0, -0.25));
			normal0Wire.Points.Add(new Point3D(0.530, 0, -0.25));

			normal0Wire.Points.Add(new Point3D(0, 0, -0.25));
			normal0Wire.Points.Add(new Point3D(0, 0.29, -0.25));

			normal0Wire.Points.Add(new Point3D(-0.530, -0.289, -0.5));
			normal0Wire.Points.Add(new Point3D(-0.530, -0.289, -0.042));

			normal0Wire.Points.Add(new Point3D(0.530, -0.289, -0.5));
			normal0Wire.Points.Add(new Point3D(0.530, -0.289, -0.042));

			normal0Wire.Points.Add(new Point3D(-0.530, 0.289, -0.5));
			normal0Wire.Points.Add(new Point3D(-0.530, 0.289, -0.042));

			normal0Wire.Points.Add(new Point3D(0.530, 0.289, -0.5));
			normal0Wire.Points.Add(new Point3D(0.530, 0.289, -0.042));

			viewport.Children.Add(normal0Wire);
		}

	}
}
