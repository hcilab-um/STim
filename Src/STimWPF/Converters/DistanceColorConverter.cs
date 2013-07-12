using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using STimAttentionWPF.Properties;

namespace STimAttentionWPF.Converters
{
	public class DistanceColorConverter : IValueConverter
	{
		const byte MAX_INTENSITY = 255;
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double distance = (double) value;
			string param = (string) parameter;
			byte colorIntensity = 0;
			
			if (distance < Settings.Default.NotificationZoneConstrain)
			{
				double distanceRatio = 1 - distance / Settings.Default.NotificationZoneConstrain;
				colorIntensity = (byte)(MAX_INTENSITY * distanceRatio);
			}

			if (param.Equals("black"))
				colorIntensity = (byte)(MAX_INTENSITY - colorIntensity);
			Brush color = new SolidColorBrush(Color.FromArgb(255, colorIntensity, colorIntensity, colorIntensity));
			return color;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
