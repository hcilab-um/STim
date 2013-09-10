using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media.Media3D;
using STimWPF.Properties;

namespace STimWPF.Converters
{
	public class HighlightSizeConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == DependencyProperty.UnsetValue || values[0] == DependencyProperty.UnsetValue
				|| values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue)
			{
				return 300;
			}

			Point3D cameraPosition = (Point3D)values[0];
			Point3D objectPosition = (Point3D)values[1];
			double objectDiameter = (double)values[2];
			
			string [] parameters = (parameter as string).Split('-');
			
			double physicalSizeRange = Settings.Default.DisplayHeightInMeters;
			double screenResolutionRange = SystemParameters.FullPrimaryScreenHeight;

			if (parameters[0].Equals("Width"))
			{
				physicalSizeRange = Settings.Default.DisplayWidthInMeters;
				screenResolutionRange = SystemParameters.FullPrimaryScreenWidth;
			}

			double eyeDistance = 0;

			if (parameters[1].Equals("Parallax"))
			{
				eyeDistance = Settings.Default.BinocularDistance;
			}

			double objectScreenDistance = -objectPosition.Z;
			double cameraScreenDistance = cameraPosition.Z;
			double objectCameraZDistance = objectScreenDistance + cameraScreenDistance;

			double physicalProjectionSize = eyeDistance * objectScreenDistance / objectCameraZDistance + objectDiameter * cameraScreenDistance / objectCameraZDistance;

			//from physical Size map to screen resolution size
			return physicalProjectionSize / physicalSizeRange * screenResolutionRange;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
