using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using STimWPF.Properties;

namespace STimWPF.Converters
{
	class DistanceRectSizeConverter : IMultiValueConverter
	{
		
		/// <values[0]: ActualWidth
		/// <values[1]: Zone
		/// <values[2]: NotificationDistance
		/// 
		/// return rectangle width
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double size;
			double distance;
			double range;
			if (values == null || values.Length != 3 || values[0] == DependencyProperty.UnsetValue)
				throw new Exception("Binding element or ActualWidth is not set up");
			
			size = (double)values[0];
			if (values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
				return size;

			if ((Interaction.InteractionZone)values[1] == Interaction.InteractionZone.Notification)
			{
				distance = (double)values[2] - Settings.Default.InteractionZoneConstrain;
				if (distance < 0)
					distance = 0;
				range = Settings.Default.NotificationZoneConstrain - Settings.Default.InteractionZoneConstrain;
				return size * distance / range;
			}
			else if ((Interaction.InteractionZone)values[1] == Interaction.InteractionZone.Interaction
				|| (Interaction.InteractionZone)values[1] == Interaction.InteractionZone.Close)
			{
				return (double)0;
			}

			return size;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new Exception("This should Never happen");
		}
	}
}
