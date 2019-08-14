using System;
using System.Globalization;
using System.Windows.Data;

namespace Netst.ValueConverters
{
    class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool v = value != null && (bool) value;
            string p = parameter as string;

            if (string.IsNullOrWhiteSpace(p)) return null;

            string[] args = p.Split('|');

            if (v) return args[0];

            return args[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
