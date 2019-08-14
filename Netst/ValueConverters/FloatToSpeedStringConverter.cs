using System;
using System.Globalization;
using System.Windows.Data;

namespace Netst.ValueConverters
{
    public class FloatToSpeedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string unit = parameter as string;
            if (string.IsNullOrEmpty(unit))
                unit = "b|true";
            bool twobased = false;

            string[] args = unit.Split('|');
            if (args.Length == 2)
            {
                unit = args[0];
                twobased = string.Equals(args[1], "true", StringComparison.OrdinalIgnoreCase);
            }

            if (value == null) return null;

            float v = float.Parse(value.ToString());

            return Netst.NetstNetworkAdapter.SpeedToString(v, unit, twobased);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
