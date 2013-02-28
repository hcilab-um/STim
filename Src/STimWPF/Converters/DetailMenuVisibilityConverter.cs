using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using STimWPF.Interaction;
using System.Windows;

namespace STimWPF.Converters
{
    public class DetailMenuVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string param = (string)parameter;
            string[] info = param.Split('_');
            if (info[1].Equals("WF"))
            {
                DetailWFPage dWF = (DetailWFPage)value;
                if (dWF != DetailWFPage.DetailMenu_WF)
                    return Visibility.Visible;
            }
            else
            {
                DetailLAPage dLA = (DetailLAPage)value;
                if (dLA != DetailLAPage.DetailMenu_LA)
                    return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
