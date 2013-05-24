using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace SpikeWPF.Converters
{
	public class HeadProjectionMatrixConverter : IValueConverter
	{
		const double HALF_WIDTH = 0.530;
		const double HALF_HEIGHT = 0.29;

		static readonly Vector3D UP_DIR = new Vector3D(0, 1, 0);

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Vector3D CamPos = (Vector3D)value;
			double zn = 0.2;
			double zf = 100;
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
