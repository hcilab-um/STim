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
using System.Windows.Media.Media3D;
using System.ComponentModel;

namespace SpikeWPF.Controls
{
	/// <summary>
	/// Interaction logic for VisualDirectionControl.xaml
	/// </summary>
	public partial class VisualDirectionControl : UserControl
	{
		public static readonly DependencyProperty HeadLocationProperty = DependencyProperty.Register("HeadLocation", typeof(Point3D), typeof(VisualDirectionControl));
		public static readonly DependencyProperty HeadOrientationProperty = DependencyProperty.Register("HeadOrientation", typeof(Vector3D), typeof(VisualDirectionControl));

		public Point3D HeadLocation
		{
			get { return (Point3D)GetValue(HeadLocationProperty); }
			set { SetValue(HeadLocationProperty, value); }
		}

		public Vector3D HeadOrientation
		{
			get { return (Vector3D)GetValue(HeadOrientationProperty); }
			set { SetValue(HeadOrientationProperty, value); }
		}

		public int Rows { get; set; }
		public int Columns { get; set; }

		public VisualDirectionControl()
		{
			Rows = 4;
			Columns = 4;
			InitializeComponent();
		}
	}
}
