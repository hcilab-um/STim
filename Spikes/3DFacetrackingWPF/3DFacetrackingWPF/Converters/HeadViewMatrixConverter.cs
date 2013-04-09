using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace KinectWPF3D.Converters
{
	public class HeadViewMatrixConverter: IValueConverter
	{
		static readonly Vector3D UP_DIR = new Vector3D(0, 1, 0);
		static readonly Vector3D LOOK_DIR = new Vector3D(0, 0, -1);
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Vector3D CamPos = (Vector3D)value;
			Vector3D lookTarget = -CamPos;
			//lookTarget.Z = 0;
			//lookTarget.Y += 0.04;

			return Math3D.SetViewMatrix((Point3D)CamPos, LOOK_DIR, UP_DIR);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
