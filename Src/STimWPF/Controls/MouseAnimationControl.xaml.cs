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
using STimWPF.Interaction;

namespace STimWPF.Controls
{
	/// <summary>
	/// Interaction logic for MouseAnimationControl.xaml
	/// </summary>
	public partial class MouseAnimationControl : UserControl
	{
		public static readonly DependencyProperty TimerStateProperty = DependencyProperty.Register("TimerState", typeof(TimerState), typeof(MouseAnimationControl));

		public TimerState TimerState
		{
			get { return (TimerState)GetValue(TimerStateProperty); }
			set { SetValue(TimerStateProperty, value); }
		}

		public MouseAnimationControl()
		{
			InitializeComponent();
		}

	}
}
