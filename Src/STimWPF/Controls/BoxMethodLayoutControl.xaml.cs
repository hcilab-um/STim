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
using STimWPF.Util;
using STimWPF.Interaction;
using STimWPF.Pointing;
using System.Windows.Media.Media3D;

namespace STimWPF.Controls
{

	/// <summary>
	/// Interaction logic for BoxMethodLayoutControl.xaml
	/// </summary>
	public partial class BoxMethodLayoutControl : UserControl
	{
		public static readonly DependencyProperty CursorPositionProperty = DependencyProperty.Register("CursorPosition", typeof(Point3D), typeof(BoxMethodLayoutControl));
		public static readonly DependencyProperty HighlightedKeyProperty = DependencyProperty.Register("HighlightedKey", typeof(Key), typeof(BoxMethodLayoutControl));
		public static readonly DependencyProperty SelectedKeyProperty = DependencyProperty.Register("SelectedKey", typeof(Key), typeof(BoxMethodLayoutControl));
		public static readonly DependencyProperty TargetKeyProperty = DependencyProperty.Register("TargetKey", typeof(Key), typeof(BoxMethodLayoutControl));
		public static readonly DependencyProperty ShowTimerProperty = DependencyProperty.Register("ShowTimer", typeof(bool), typeof(BoxMethodLayoutControl));
		public static readonly DependencyProperty TimerStateProperty = DependencyProperty.Register("TimerState", typeof(TimerState), typeof(BoxMethodLayoutControl));

		public Point3D CursorPosition
		{
			get { return (Point3D)GetValue(CursorPositionProperty); }
			set { SetValue(CursorPositionProperty, value); }
		}

		public Key HighlightedKey
		{
			get { return (Key)GetValue(HighlightedKeyProperty); }
			set { SetValue(HighlightedKeyProperty, value); }
		}

		public Key SelectedKey
		{
			get { return (Key)GetValue(SelectedKeyProperty); }
			set { SetValue(SelectedKeyProperty, value); }
		}

		public Key TargetKey
		{
			get { return (Key)GetValue(TargetKeyProperty); }
			set { SetValue(TargetKeyProperty, value); }
		}

		public bool ShowTimer
		{
			get { return (bool)GetValue(ShowTimerProperty); }
			set { SetValue(ShowTimerProperty, value); }
		}

		public TimerState TimerState
		{
			get { return (TimerState)GetValue(TimerStateProperty); }
			set { SetValue(TimerStateProperty, value); }
		}

		public BoxMethodLayoutControl()
		{
			InitializeComponent();
			HighlightedKey = Key.None;
			SelectedKey = Key.None;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (!this.Visibility.Equals(Visibility.Visible))
				return;
			if (e.Property == BoxMethodLayoutControl.CursorPositionProperty)
			{
				Canvas.SetLeft(eCursor, ActualWidth * CursorPosition.X - eCursor.ActualWidth / 2);
				Canvas.SetTop(eCursor, ActualHeight * CursorPosition.Y - eCursor.ActualHeight / 2);
				Canvas.SetLeft(eCursorLock, ActualWidth * CursorPosition.X - eCursorLock.ActualWidth / 2);
				Canvas.SetTop(eCursorLock, ActualHeight * CursorPosition.Y - eCursorLock.ActualHeight / 2);
				return;
			}
		}
	}
}
