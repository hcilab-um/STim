using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace SpikeWPF.Converters
{
	class HeadConeLocationConverter : IValueConverter
	{
		const double DISPLAY_CENTER_X = 583;
		const double DISPLAY_CENTER_Y = 384;
		
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Vector3D headV = (Vector3D)value;
			string param = (string)parameter;
			double offset=0;
			if (param.Equals("Left"))
			{
				double physicalShiftX = headV.X - headV.X * headV.Z / (headV.Z + 0.5);
				double graphicalShiftX = physicalShiftX / 0.531 * DISPLAY_CENTER_X;
				offset = DISPLAY_CENTER_X + graphicalShiftX;
				return offset;
			}

			if(param.Equals("Top"))
			{
				double physicalShiftY = headV.Y - headV.Y * headV.Z / (headV.Z + 0.5);
				double graphicalShiftY = physicalShiftY / 0.29 * DISPLAY_CENTER_Y;
				offset = DISPLAY_CENTER_Y - graphicalShiftY;
				return offset;
			}

			throw new Exception("Parameter wrong");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
