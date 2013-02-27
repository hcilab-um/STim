using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using STimWPF.Interaction;
using System.Windows;

namespace STimWPF.Converters
{
	class DetailLAPageVisibilityConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			DetailLAPage dLAP = (DetailLAPage)value;
			string param = (string)parameter;
			if (dLAP.ToString().Equals(param))
				return Visibility.Visible;
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
