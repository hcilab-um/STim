using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media.Media3D;
using SpikeWPF.Properties;

namespace SpikeWPF.Converters
{
	public class LocationOrientationViewConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Thickness th = new Thickness(){ Left =0, Top = 0};
			if (values == DependencyProperty.UnsetValue || values[0] == DependencyProperty.UnsetValue
				|| values[1] == DependencyProperty.UnsetValue)
				return th;
			Vector3D headLocation = (Vector3D)values[0];
			Vector3D headOrientation = (Vector3D)values[1]; 
			double width = 1920;
			double height = 1080;

			double rational = -headLocation.Z / headOrientation.Z;

			double displayLocationX = headOrientation.X * rational + headLocation.X;
			double displayLocationY = headOrientation.Y *rational +headLocation.Y - 0.14;

			th.Left = displayLocationX / Settings.Default.DisplayWidthInMeters * width + width / 2 - 100;
			th.Left = Math.Max(th.Left, 0);
			th.Left = Math.Min(th.Left, width-100);
			th.Top = height/2 - displayLocationY / Settings.Default.DisplayHeightInMeters * height;
			th.Top = Math.Max(th.Top, 0);
			th.Top = Math.Min(th.Top, height-100);
			return th;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
