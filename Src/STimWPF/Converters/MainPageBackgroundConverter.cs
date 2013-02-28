using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using STimWPF.Interaction;
using System.Windows.Media;

namespace STimWPF.Converters
{
    public class MainPageBackgroundConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MainPage mp = (MainPage)value;
            string param = (string)parameter;
            if (mp.ToString().Equals(param))
                return Brushes.SkyBlue;
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
