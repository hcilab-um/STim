using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using STimWPF.Interaction;

namespace STimWPF.Converters
{
	public class ZoneVisibilityConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Zone zone = (Zone)value;
			string param = (String)parameter;

			String[] zones = param.Split('-');
			
			foreach (String zoneParam in zones)
			{
				if (zoneParam.Equals(zone.ToString()))
					return Visibility.Visible;
			}

			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

	}
}
