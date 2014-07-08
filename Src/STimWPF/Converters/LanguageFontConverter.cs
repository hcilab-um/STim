using STimWPF.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace STimWPF.Converters
{
  public class LanguageFontConverter : IValueConverter
  {

    private FontFamily FontForLatin { get; set; }
    private FontFamily FontForInuit { get; set; }

    public LanguageFontConverter()
    {
      FontForLatin = new FontFamily("Heveltica");
      FontForInuit = new FontFamily("nunacom");
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      FontType type = (FontType)value;
      if (type == FontType.Latin)
        return FontForLatin;
      return FontForInuit;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
