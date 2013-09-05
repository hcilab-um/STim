using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace STimWPF.Converters
{
	public class BoolVisibilityConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isTrue = (bool)value;

			string statement = (string)parameter;

			if(statement.Equals("ToShow") ^ isTrue)
			{
				return Visibility.Hidden;
			}

			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
