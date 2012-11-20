using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using STimWPF.Interaction;
using STimWPF.Pointing;

namespace STimWPF.Converters
{
	class MethodVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ("Timer".Equals(parameter as String) && (bool)value)
				return Visibility.Visible;
			if ("Box".Equals(parameter as String) && (value is BoxInteractionMethod))
				return Visibility.Visible;
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

	}
}
