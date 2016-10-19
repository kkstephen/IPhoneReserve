using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Globalization;

namespace IR.Common
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            // value is the data from the source object.
            var b = (bool)value;

            if (b)
                return "Assets/click.png";
            else
                return "Assets/non-click.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Do the conversion from visibility to bool
            return null;
        }
    }
}
