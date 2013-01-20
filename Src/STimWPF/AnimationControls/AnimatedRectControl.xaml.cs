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
using System.Windows.Threading;

namespace STimWPF.AnimationControls
{
	/// <summary>
	/// Interaction logic for AnimatedRectControl.xaml
	/// </summary>
	public partial class AnimatedRectControl : UserControl
	{
		public static readonly RoutedEvent TimerElapsedEvent = EventManager.RegisterRoutedEvent("TimerElapsed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AnimatedRectControl));

		public static readonly DependencyProperty ZoneProperty = DependencyProperty.Register("Zone", typeof(Interaction.InteractionZone), typeof(AnimatedRectControl));
		public static readonly DependencyProperty NotificationDistanceProperty = DependencyProperty.Register("NotificationDistance", typeof(double), typeof(AnimatedRectControl));

		private const int MAX_ELAPSED_TIME_TICK = 3000;

		public Interaction.InteractionZone Zone
		{
			get { return (Interaction.InteractionZone)GetValue(ZoneProperty); }
			set { SetValue(ZoneProperty, value); }
		}

		public double NotificationDistance
		{
			get { return (double)GetValue(NotificationDistanceProperty); }
			set { SetValue(NotificationDistanceProperty, value); }
		}

		// Provide CLR accessors for the event 
		public event RoutedEventHandler TimerElapsed
		{
			add { AddHandler(TimerElapsedEvent, value); }
			remove { RemoveHandler(TimerElapsedEvent, value); }
		}

		private Random rGenerator = null;
		private System.Timers.Timer timer = null;

		public AnimatedRectControl()
		{
			rGenerator = new Random((int)DateTime.Now.Ticks % 1000);
			timer = new System.Timers.Timer();
			timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
			InitializeComponent();
		}

		private void RectControl_Loaded(object sender, RoutedEventArgs e)
		{
			timer.Interval = rGenerator.Next() % MAX_ELAPSED_TIME_TICK + 1;
			timer.Enabled = true;
			timer.Start();
		}

		void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			timer.Stop();
			Dispatcher.Invoke(DispatcherPriority.Render, (Action)delegate()
			{
				if (Zone != Interaction.InteractionZone.Ambient)
					return;

				animatedRect.RaiseEvent(new RoutedEventArgs(AnimatedRectControl.TimerElapsedEvent, this));
			});
		}

		private void Animation_Completed(object sender, EventArgs e)
		{
			timer.Interval = rGenerator.Next() % MAX_ELAPSED_TIME_TICK + 1;
			timer.Start();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property != AnimatedRectControl.ZoneProperty)
				return;

			if (Zone == Interaction.InteractionZone.Ambient)
			{
				timer.Interval = rGenerator.Next() % MAX_ELAPSED_TIME_TICK + 1;
				timer.Start();
			}
			else
			{
				//trick from: http://joshsmithonwpf.wordpress.com/2008/08/21/removing-the-value-applied-by-an-animation/
				animatedRect.BeginAnimation(Rectangle.WidthProperty, null);
				animatedRect.BeginAnimation(Rectangle.HeightProperty, null);
			}
		}
	}
}
