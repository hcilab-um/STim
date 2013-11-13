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

namespace SpikeWPF.Controls
{
	/// <summary>
	/// Interaction logic for TransparencyControl.xaml
	/// </summary>
	public partial class TransparencyControl : UserControl
	{
		public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register("Distance", typeof(double), typeof(TransparencyControl));

		public double Distance
		{
			get { return (double)GetValue(DistanceProperty); }
			set { SetValue(DistanceProperty, value); }
		}

		public TransparencyControl()
		{
			Distance = 2;
			InitializeComponent();
		}

	}
}
