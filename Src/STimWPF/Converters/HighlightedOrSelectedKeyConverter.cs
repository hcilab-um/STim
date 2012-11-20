using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;

namespace STimWPF.Converters
{
	class HighlightedOrSelectedKeyConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == DependencyProperty.UnsetValue || values.Length < 3 || values.Length > 4)
				return Brushes.Transparent;
			if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
				return Brushes.Transparent;
			if (values[2] == null)
				return Brushes.Transparent;

			Key highlightedKey = (Key)values[0];
			Key selectedKey = (Key)values[1];
			Key keyIcon = (Key)Enum.Parse(typeof(Key), (String)values[2]);
			Key targetKey;

			if (values.Length == 4 && values[3] != null)
			{
				targetKey = (Key)values[3];
				if (keyIcon == targetKey)
				{
					if (selectedKey != targetKey)
					{
						if (targetKey == highlightedKey)
							return Brushes.YellowGreen;
						return Brushes.SkyBlue;
					}
				}
			}

			if (selectedKey == keyIcon)
				return Brushes.Red;
			else if (highlightedKey == keyIcon)
				return Brushes.Pink;
			return Brushes.Transparent;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
