using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using STimWPF.Graphic3D;
using STimWPF.Properties;

namespace STimWPF.Converters
{
	public class HeadProjectionMatrixConverter : IValueConverter
	{		
		static readonly Vector3D UP_DIR = new Vector3D(0, 1, 0);

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Point3D CamPos = (Point3D)value;
			double zn = 0.1;
			double zf = 100;

			double left = zn * (-Settings.Default.DisplayWidthInMeters / 2 - CamPos.X) / CamPos.Z;
			double right = zn * (Settings.Default.DisplayWidthInMeters / 2 - CamPos.X) / CamPos.Z;
			double bottom = zn * (-Settings.Default.DisplayHeightInMeters / 2 - CamPos.Y) / CamPos.Z;
			double top = zn * (Settings.Default.DisplayHeightInMeters / 2 - CamPos.Y + Settings.Default.CenterOffsetY) / CamPos.Z;
			return Math3D.SetPerspectiveOffCenter(left, right, bottom, top, zn, zf);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
