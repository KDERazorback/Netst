using System;
using System.Globalization;
using System.Windows.Data;

namespace Netst.ValueConverters
{
    public class BooleanToYesNoValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool v = (value != null && (bool) value);

            return v ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
