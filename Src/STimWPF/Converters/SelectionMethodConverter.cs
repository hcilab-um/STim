using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using STimWPF.Interaction;

namespace STimWPF.Converters
{
	public class SelectionMethodConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			SelectionMethod current = (SelectionMethod)value;
			SelectionMethod target = (SelectionMethod)Enum.Parse(typeof(SelectionMethod), parameter as String);
			return current == target;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			SelectionMethod target = (SelectionMethod)Enum.Parse(typeof(SelectionMethod), parameter as String);
			return target;
		}
	}
}
