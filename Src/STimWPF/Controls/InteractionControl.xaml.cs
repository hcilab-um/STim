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
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace STimWPF.Controls
{
	/// <summary>
	/// Interaction logic for InteractionControl.xaml
	/// </summary>
	public partial class InteractionControl : UserControl, INotifyPropertyChanged
	{
		private ContentState contentState;
		private DetailContentState detailContentState;
		
		private static readonly DependencyProperty LeftClickProperty = DependencyProperty.Register("LeftClick", typeof(bool), typeof(InteractionControl));
		private static readonly DependencyProperty RelativeCursorLocationProperty = DependencyProperty.Register("RelativeCursorLocation", typeof(Point3D), typeof(InteractionControl));
		private static readonly DependencyProperty TimerStateProperty = DependencyProperty.Register("TimerState", typeof(TimerState), typeof(InteractionControl));
		
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

		public ContentState ContentState
		{
			get { return contentState; }
			set
			{
				contentState = value;
				OnPropertyChanged("ContentState");
			}
		}

		public DetailContentState DetailContentState
		{
			get { return detailContentState; }
			set
			{
				detailContentState = value;
				OnPropertyChanged("DetailContentState");
			}
		}

		public InteractionControl()
		{
			InitializeComponent();
		}

		void Overview_Clicked(object sender, EventArgs e)
		{
			ContentState = ContentState.Overview;
		}

		private void Descrption_Clicked(object sender, RoutedEventArgs e)
		{
			ContentState = ContentState.Description;
		}

		private void Detail_Clicked(object sender, RoutedEventArgs e)
		{
			ContentState = ContentState.Detail;
		}

		private void PartA_Click(object sender, RoutedEventArgs e)
		{
			DetailContentState = DetailContentState.PartA;
		}

		private void PartB_Click(object sender, RoutedEventArgs e)
		{
			DetailContentState = DetailContentState.PartB;
		}

		private void PartC_Click(object sender, RoutedEventArgs e)
		{
			DetailContentState = DetailContentState.PartC;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
