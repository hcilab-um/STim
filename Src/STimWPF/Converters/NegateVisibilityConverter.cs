using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace STimWPF.Converters
{
	class NegateVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == DependencyProperty.UnsetValue)
				return Visibility.Visible;

			Visibility isVisible = (Visibility)value;
			if (isVisible == Visibility.Visible)
				return Visibility.Hidden;
			else
				return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
