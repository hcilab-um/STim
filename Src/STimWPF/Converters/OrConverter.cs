using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace STimWPF.Converters
{
	class OrConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == DependencyProperty.UnsetValue || values[0] == DependencyProperty.UnsetValue
				|| values[1] == DependencyProperty.UnsetValue)
				return false;

			String keys = (String)parameter;
			if (keys.Length != values.Length)
				throw new Exception("keys.Length != values.Length");

			for (int index = 0; index < values.Length; index++)
			{
				char key = keys[index];
				bool value = (bool)values[index];
				if (key == 'F' && !value)
					return true;
				if (key == 'T' && value)
					return true;
			}
			return false;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
