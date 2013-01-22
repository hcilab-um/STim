using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using STimWPF.Interaction;
using System.Windows;

namespace STimWPF.Converters
{
	class DetailContentStateVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			DetailContentState c = (DetailContentState)value;
			string param = (string)parameter;
			if (param.Equals("A") && c == DetailContentState.PartA)
				return Visibility.Visible;
			if (param.Equals("B") && c == DetailContentState.PartB)
				return Visibility.Visible;
			if (param.Equals("C") && c == DetailContentState.PartC)
				return Visibility.Visible;
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
