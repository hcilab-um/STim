using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace STimWPF.Converters
{
	class AndConverter : IMultiValueConverter
	{
		/// <summary>
		/// And Converter - Makes an AND operation among all the values. Each value is previously transformed by the mask.1
		/// </summary>
		/// <param name="values"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == DependencyProperty.UnsetValue || values[0] == DependencyProperty.UnsetValue
				|| values[1] == DependencyProperty.UnsetValue)
				return true;

			String keys = (String)parameter;
			if (keys.Length != values.Length)
				throw new Exception("keys.Length != values.Length");

			for (int index = 0; index < keys.Length; index++)
			{
				char key = keys[index];
				bool value = (bool)values[index];
				if (key == 'F' && !value)
					continue;
				if (key == 'T' && value)
					continue;
				return false;
			}

			return true;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
