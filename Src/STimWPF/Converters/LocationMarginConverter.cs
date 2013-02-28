using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media.Media3D;

namespace STimWPF.Converters
{
	class LocationMarginConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Thickness th = new Thickness();
			Point3D location = (Point3D)value;
			th.Left = location.X -30;
			th.Top = location.Y-30;
			return th;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
