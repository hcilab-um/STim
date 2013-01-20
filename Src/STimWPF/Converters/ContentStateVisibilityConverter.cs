using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using STimWPF.Interaction;
using System.Windows;

namespace STimWPF.Converters
{
	class ContentStateVisibilityConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			ContentState c = (ContentState)value;
			string param = (string)parameter;
			if (param.Equals("overview")&& c ==ContentState.Overview)
				return Visibility.Visible;
			if (param.Equals("description") && c == ContentState.Description)
				return Visibility.Visible;
			if (param.Equals("detail") && c == ContentState.Detail)
				return Visibility.Visible;
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
