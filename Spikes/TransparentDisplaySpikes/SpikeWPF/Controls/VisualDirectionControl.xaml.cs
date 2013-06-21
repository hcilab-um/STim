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

namespace SpikeWPF.Controls
{
	/// <summary>
	/// Interaction logic for VisualDirectionControl.xaml
	/// </summary>
	public partial class VisualDirectionControl : UserControl
	{
		public static readonly DependencyProperty HeadLocationVProperty = DependencyProperty.Register("HeadLocationV", typeof(Vector3D), typeof(VisualDirectionControl));
		public static readonly DependencyProperty HeadOrientationProperty = DependencyProperty.Register("HeadOrientationV", typeof(Vector3D), typeof(VisualDirectionControl));

		public Vector3D HeadLocationV
		{
			get { return (Vector3D)GetValue(HeadLocationVProperty); }
			set { SetValue(HeadLocationVProperty, value); }
		}

		public Vector3D HeadOrientationV
		{
			get { return (Vector3D)GetValue(HeadOrientationProperty); }
			set { SetValue(HeadOrientationProperty, value); }
		}

		public VisualDirectionControl()
		{
			InitializeComponent();
		}

	}
}
