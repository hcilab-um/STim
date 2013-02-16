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
using System.Windows.Media.Media3D;

namespace STimWPF.Controls
{
	/// <summary>
	/// Interaction logic for InteractionControl.xaml
	/// </summary>
	public partial class InteractionControl : UserControl
	{
		
		private static readonly DependencyProperty LeftClickProperty = DependencyProperty.Register("LeftClick", typeof(bool), typeof(InteractionControl));
		private static readonly DependencyProperty RelativeCursorLocationProperty = DependencyProperty.Register("RelativeCursorLocation", typeof(Point3D), typeof(InteractionControl));
		private static readonly DependencyProperty TimerStateProperty = DependencyProperty.Register("TimerState", typeof(TimerState), typeof(InteractionControl));
		private static readonly DependencyProperty ContentStateProperty = DependencyProperty.Register("ContentState", typeof(ContentState), typeof(InteractionControl));
		private static readonly DependencyProperty DetailContentStateProperty = DependencyProperty.Register("DetailContentState", typeof(DetailContentState), typeof(InteractionControl));

		public ContentState ContentState
		{
			get { return (ContentState)GetValue(ContentStateProperty); }
			set { SetValue(ContentStateProperty, value); }
		}

		public DetailContentState DetailContentState
		{
			get { return (DetailContentState)GetValue(DetailContentStateProperty); }
			set { SetValue(DetailContentStateProperty, value); }
		}

		public bool	LeftClick
		{
			get { return (bool)GetValue(LeftClickProperty); }
			set { SetValue(LeftClickProperty, value); }
		}

		public Point3D RelativeCursorLocation
		{
			get { return (Point3D)GetValue(RelativeCursorLocationProperty); }
			set { SetValue(RelativeCursorLocationProperty, value); }
		}

		public TimerState TimerState
		{
			get { return (TimerState)GetValue(TimerStateProperty); }
			set { SetValue(TimerStateProperty, value); }
		}

		public InteractionControl()
		{
			InitializeComponent();
		}

		void Overview_Clicked(object sender, EventArgs e)
		{
			Core.Instance.ContentState = ContentState.Overview;
		}

		private void Descrption_Clicked(object sender, RoutedEventArgs e)
		{
			Core.Instance.ContentState = ContentState.Description;
		}

		private void Detail_Clicked(object sender, RoutedEventArgs e)
		{
			Core.Instance.ContentState = ContentState.Detail;
		}

		private void PartA_Click(object sender, RoutedEventArgs e)
		{
			Core.Instance.DetailContentState = DetailContentState.PartA;
		}

		private void PartB_Click(object sender, RoutedEventArgs e)
		{
			Core.Instance.DetailContentState = DetailContentState.PartB;
		}

		private void PartC_Click(object sender, RoutedEventArgs e)
		{
			Core.Instance.DetailContentState = DetailContentState.PartC;
		}
	}
}
