using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Netst.ValueConverters
{
    public class BooleanToImageConverter : IValueConverter
    {
        public static Uri BasePath;

        public virtual string TrueResource { get; set; } = "Resources/green.png";
        public virtual string FalseResource { get; set; } = "Resources/red.png";

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

            string _TrueResource = TrueResource;
            string _FalseResource = FalseResource;

            if (!string.IsNullOrWhiteSpace((string)parameter))
            {
                string p = (string) parameter;
                string[] args = p.Split('|');

                _TrueResource = args[0];

                if (args.Length > 1)
                    _FalseResource = args[1];
            }

            bool v = value != null && (bool)value;
            BitmapImage output;

            if (v)
                output = new BitmapImage(new Uri(BasePath, _TrueResource));
            else
                output = new BitmapImage(new Uri(BasePath, _FalseResource));

            output.CacheOption = BitmapCacheOption.None;
            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
