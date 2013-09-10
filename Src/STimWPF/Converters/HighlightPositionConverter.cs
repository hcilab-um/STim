using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using STimWPF.Properties;
using System.Windows;

namespace STimWPF.Converters
{
	public class HighlightPositionConverter: IMultiValueConverter
	{

		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == DependencyProperty.UnsetValue || values[0] == DependencyProperty.UnsetValue
				|| values[1] == DependencyProperty.UnsetValue)
			{
				return new System.Windows.Thickness(0, 0, 0, 0);
			}

			Point3D cameraPosition = (Point3D)values[0];
			Point3D objectPosition = (Point3D)values[1];
			double projectionX = CalculateProjection(objectPosition.X, cameraPosition.X, cameraPosition.Z, -objectPosition.Z);
			double projectionY = CalculateProjection(objectPosition.Y, cameraPosition.Y, cameraPosition.Z, -objectPosition.Z);
			double leftMargin = projectionX / Settings.Default.DisplayWidthInMeters*2 * SystemParameters.PrimaryScreenWidth;
			double bottomMargin = projectionY / Settings.Default.DisplayHeightInMeters*2 * SystemParameters.PrimaryScreenHeight;
			return new System.Windows.Thickness(leftMargin, 0, 0, bottomMargin);
		}

		private double CalculateProjection(double objectPos, double cameraPos, double cameraScreenDistance, double objectScreenDistance)
		{
			return (cameraPos - objectPos) * objectScreenDistance / (cameraScreenDistance + objectScreenDistance) + objectPos;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
