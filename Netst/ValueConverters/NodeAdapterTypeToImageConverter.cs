using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Netst.NetstApi.Broadcast;
using Netst.Pages;

namespace Netst.ValueConverters
{
    public class NodeAdapterTypeToImageConverter : IValueConverter
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


            NodeInfo node = value as NodeInfo;
            if (node == null)
                return null;

            string adapterType = node.GetExtraParam("adapter-type");

            if (string.Equals(adapterType, NetworkAdapterTypeToStringConverter.EthernetName, StringComparison.OrdinalIgnoreCase))
                return new BitmapImage(new Uri(BasePath, "Resources/ethernet.png"));

            if (string.Equals(adapterType, NetworkAdapterTypeToStringConverter.RadioName, StringComparison.OrdinalIgnoreCase))
                return new BitmapImage(new Uri(BasePath, "Resources/wireless.png"));

            if (string.Equals(adapterType, NetworkAdapterTypeToStringConverter.FiberName, StringComparison.OrdinalIgnoreCase))
                return new BitmapImage(new Uri(BasePath, "Resources/fiber.png"));

            return new BitmapImage(new Uri(BasePath, "Resources/question.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
