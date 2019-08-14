using System;
using System.Globalization;
using System.Windows.Data;

namespace Netst.ValueConverters
{
    public class NetworkAdapterTypeToStringConverter : IValueConverter
    {
        public static string EthernetName = "Ethernet adapter";
        public static string RadioName = "Wireless Radio adapter";
        public static string FiberName = "Optic Fiberglass adapter";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            NetstNetworkAdapter adapter = value as NetstNetworkAdapter;

            if (value == null || adapter == null)
                return null;

            if (adapter.IsEthernet)
                return EthernetName;

            if (adapter.IsRadio)
                return RadioName;

            if (adapter.IsFiber)
                return FiberName;

            return "Unknown adapter type.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
