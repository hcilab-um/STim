using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using STimWPF.Util;

namespace STimWPF.Converters
{
	public class TimerLocationConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values.Length != 4 ||
				values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue ||
				values[2] == DependencyProperty.UnsetValue || values[3] == DependencyProperty.UnsetValue)
				return 0;

			String type = parameter as String;
			if ("Left".Equals(type))
			{
				double targetWidth = (double)values[0];
				double areaWidth = (double)values[1];
				double locationX = ((Point3D)values[2]).X;
				double timerWidth = (double)values[3];

				double left = areaWidth * locationX - timerWidth / 2;
				return left;
			}
			else if ("Top".Equals(type))
			{
				double targetHeight = (double)values[0];
				double areaHeight = (double)values[1];
				double locationY = ((Point3D)values[2]).Y;
				double timerHeight = (double)values[3];

				double top = areaHeight * locationY - timerHeight / 2;
				return top;
			}

			return 1;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
