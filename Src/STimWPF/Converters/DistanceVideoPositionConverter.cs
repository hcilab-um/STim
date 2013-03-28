using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using STimWPF.Properties;

namespace STimWPF.Converters
{
    class DistanceVideoPositionConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double userDisplayDistance = (double)value;
            double relativeProgress = 0;
            double beginProgress = 5;
            double range = Settings.Default.NotificationZoneConstrain - Settings.Default.InteractionZoneConstrain;
            if(userDisplayDistance >= Settings.Default.InteractionZoneConstrain)
            {                    
                relativeProgress = userDisplayDistance - Settings.Default.InteractionZoneConstrain;
            }
            int actualProgress = (int)((relativeProgress / range + beginProgress) * 1000);
            var ts = TimeSpan.FromMilliseconds(actualProgress);
            return ts;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
