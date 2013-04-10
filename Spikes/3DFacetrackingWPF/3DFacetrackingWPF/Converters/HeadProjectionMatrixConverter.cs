using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace KinectWPF3D.Converters
{
	public class HeadProjectionMatrixConverter : IValueConverter
	{
		const double HALF_WIDTH = 0.5375;
		const double HALF_HEIGHT = 0.29;

		static readonly Vector3D UP_DIR = new Vector3D(0, 1, 0);

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Vector3D CamPos = (Vector3D)value;
			CamPos.Y -= 0.04;
			double zn = 1;
			double zf = 10;
			double left = zn * (-HALF_WIDTH - CamPos.X) / CamPos.Z;
			double right = zn * (HALF_WIDTH - CamPos.X) / CamPos.Z;
			double bottom = zn * (-HALF_HEIGHT - CamPos.Y) / CamPos.Z;
			double top = zn * (HALF_HEIGHT - CamPos.Y) / CamPos.Z;
			return Math3D.SetPerspectiveOffCenter(left, right, bottom, top, zn, zf);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
