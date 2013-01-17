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
			InteractionZone zone = (InteractionZone)value;
			string param = (String)parameter;
			if (!param.Equals("Interaction"))
			{
				if (zone == InteractionZone.Interaction || zone == InteractionZone.Close)
					return Visibility.Hidden;
				else
					return Visibility.Visible;
			}
			else
			{
				if (zone == InteractionZone.Close)
					return Visibility.Hidden;
				else
					return Visibility.Visible;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
