using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media.Media3D;

namespace STim.Converters
{
	public class LocationOrientationViewConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Thickness th = new Thickness(){ Left =0, Top = 0};
			if (values == DependencyProperty.UnsetValue || values[0] == DependencyProperty.UnsetValue
				|| values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue 
				|| values[3] == DependencyProperty.UnsetValue || values[4] == DependencyProperty.UnsetValue
				|| values[5] == DependencyProperty.UnsetValue)
				return th;
			Point3D headLocation = (Point3D)values[0];
			Vector3D headOrientation = (Vector3D)values[1];

			double width = (double)values[2];
			double height = (double)values[3];
			
			int rowCount = (int)values[4];
			int colCount = (int)values[5];
			
			double rectWidth = width / colCount;
			double rectHeight = height / rowCount;

			double displayPhysicalLocationX = Double.MaxValue;
			double displayPhysicalLocationY = Double.MaxValue;
			
			if (headOrientation.Z != 0)
			{
				double rational = -headLocation.Z / headOrientation.Z;
				displayPhysicalLocationX = headOrientation.X * rational + headLocation.X;
				displayPhysicalLocationY = headOrientation.Y * rational + headLocation.Y;
			}

			int column = (int)((displayPhysicalLocationX / STimSettings.DisplayWidthInMeters * width +width/2) / rectWidth);
			int row = (int)((-displayPhysicalLocationY / STimSettings.DisplayHeightInMeters * height + height / 2) / rectHeight);
			
			column = Math.Max(0, column);
			column = Math.Min(column, colCount - 1);
			row = Math.Max(0, row);
			row = Math.Min(row, rowCount - 1);

			th.Left = column * rectWidth;
			th.Top = row * rectHeight;
			return th;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
