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
			double zn = 1;
			double zf = 10;
			double aspectRatio = HALF_WIDTH / HALF_HEIGHT;
			double left = zn * (-0.28 * aspectRatio - CamPos.X) / CamPos.Z;
			double right = zn * (0.28 * aspectRatio - CamPos.X) / CamPos.Z;
			double bottom = zn * (-0.28 - CamPos.Y) / CamPos.Z;
			double top = zn * (0.28 - CamPos.Y) / CamPos.Z;
			return Math3D.SetPerspectiveOffCenter(left, right, bottom, top, zn, zf);
			//return Math3D.SetPerspectiveOffCenter(-HALF_WIDTH, HALF_WIDTH, -HALF_HEIGHT, HALF_HEIGHT, zn, zf);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
