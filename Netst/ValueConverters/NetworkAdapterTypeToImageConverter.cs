using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Netst.ValueConverters
{
    public class NetworkAdapterTypeToImageConverter : IValueConverter
    {
        public static Uri BasePath = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (BasePath == null)
            {
                Assembly entry = Assembly.GetEntryAssembly();
                if (entry != null)
                {
                    FileInfo fi = new FileInfo(entry.Location);
                    BasePath = new Uri("file://" + fi.Directory?.FullName + "/", UriKind.Absolute);
                }
                else
                    BasePath = new Uri("./", UriKind.Relative);
            }

            NetstNetworkAdapter adapter = value as NetstNetworkAdapter;

            if (value == null)
                return null;

            if (adapter.IsEthernet)
                return new BitmapImage(new Uri(BasePath, "Resources/ethernet.png"));

            if (adapter.IsRadio)
                return new BitmapImage(new Uri(BasePath, "Resources/wireless.png"));

            if (adapter.IsFiber)
                return new BitmapImage(new Uri(BasePath, "Resources/fiber.png"));

            return new BitmapImage(new Uri(BasePath, "Resources/question.png"));
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
