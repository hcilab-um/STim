using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using STimWPF.Graphic3D;

namespace STimWPF.Converters
{
	public class HeadViewMatrixConverter: IValueConverter
	{
		static readonly Vector3D UP_DIR = new Vector3D(0, 1, 0);
		static readonly Vector3D LOOK_DIR = new Vector3D(0, 0, -1);

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Point3D CamPos = (Point3D)value;
			return Math3D.SetViewMatrix(CamPos, LOOK_DIR, UP_DIR);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

	}
}
