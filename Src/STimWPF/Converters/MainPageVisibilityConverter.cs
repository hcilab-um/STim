using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using STimWPF.Interaction;
using System.Windows;

namespace STimWPF.Converters
{
	class MainPageVisibilityConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			MainPage c = (MainPage)value;
			string param = (string)parameter;
			if (param.Equals("Overview")&& c ==MainPage.Overview)
				return Visibility.Visible;
			if (param.Equals("Author") && c == MainPage.Author)
				return Visibility.Visible;
			if (param.Equals("Detail") && c == MainPage.Detail)
				return Visibility.Visible;
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
