using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace STimWPF.Converters
{
  class ZoneVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
      STim.Interaction.Zone targetZone = (STim.Interaction.Zone)Enum.Parse(typeof(STim.Interaction.Zone), parameter as String);
      STim.Interaction.Zone zone = (STim.Interaction.Zone)value;
			if (targetZone == zone)
				return Visibility.Visible;
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
