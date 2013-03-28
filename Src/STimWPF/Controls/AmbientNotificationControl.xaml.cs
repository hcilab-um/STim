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
using STimWPF.Properties;
using STimWPF.Interaction;

namespace STimWPF.Controls
{
	/// <summary>
	/// Interaction logic for AmbientNotificationControl.xaml
	/// </summary>
	public partial class AmbientNotificationControl : UserControl
	{
		public static readonly DependencyProperty UserDisplayDistanceProperty = DependencyProperty.Register("UserDisplayDistance", typeof(double), typeof(AmbientNotificationControl));
		public static readonly DependencyProperty ZoneProperty = DependencyProperty.Register("Zone", typeof(Zone), typeof(AmbientNotificationControl));

		public double UserDisplayDistance
		{
			get { return (double)GetValue(UserDisplayDistanceProperty); }
			set { SetValue(UserDisplayDistanceProperty, value); }
		}

		public Zone Zone
		{
			get { return (Zone)GetValue(ZoneProperty); }
			set { SetValue(ZoneProperty, value); }
		}

		public AmbientNotificationControl()
		{
			InitializeComponent();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == AmbientNotificationControl.UserDisplayDistanceProperty)
			{
				if (UserDisplayDistance >= Settings.Default.NotificationZoneConstrain || UserDisplayDistance <= Settings.Default.InteractionZoneConstrain)
					return;
				double range = Settings.Default.NotificationZoneConstrain - Settings.Default.InteractionZoneConstrain;
				double relativeProgress = 1-(UserDisplayDistance - Settings.Default.InteractionZoneConstrain)/range;
				int actualProgress = (int)(relativeProgress * 20000);
				TimeSpan ts = new TimeSpan(0, 0, 0, 0, actualProgress);
				//meNotification.Stop();
				meNotification.Position = ts;
				meNotification.Play();
				meNotification.Pause();
			}
		}

	}
}
