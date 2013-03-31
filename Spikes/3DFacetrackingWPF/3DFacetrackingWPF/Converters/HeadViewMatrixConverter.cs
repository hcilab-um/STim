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
 
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Vector3D headV = (Vector3D)value;
			Vector3D vN = -headV;
			Vector3D vU = Vector3D.CrossProduct(UP_DIR,vN); 
			Vector3D vV = Vector3D.CrossProduct(vN, vU);
			Matrix3D mView = new Matrix3D(vU.X, vU.Y, vU.Z, 0,
																		vV.X, vV.Y, vV.Z, 0,
																		vN.X, vN.Y, vN.Z, Vector3D.DotProduct(headV, vN),
																		0,		0,		0,		1);
			return mView;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
