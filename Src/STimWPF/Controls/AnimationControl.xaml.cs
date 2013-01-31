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

namespace STimWPF.Controls
{
	/// <summary>
	/// Interaction logic for AnimationControl.xaml
	/// </summary>
	public partial class AnimationControl : UserControl
	{
		public static readonly DependencyProperty ZoneProperty = DependencyProperty.Register("Zone", typeof(Interaction.Zone), typeof(AnimationControl));
		public static readonly DependencyProperty UserDisplayDistanceProperty = DependencyProperty.Register("UserDisplayDistance", typeof(double), typeof(AnimationControl));

		public Interaction.Zone Zone
		{
			get { return (Interaction.Zone)GetValue(ZoneProperty); }
			set { SetValue(ZoneProperty, value); }
		}

		public double UserDisplayDistance
		{
			get { return (double)GetValue(UserDisplayDistanceProperty); }
			set { SetValue(UserDisplayDistanceProperty, value); }
		}

		public AnimationControl()
		{
			InitializeComponent();
		}

	}
}
