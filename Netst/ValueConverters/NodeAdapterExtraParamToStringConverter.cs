using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Netst.NetstApi.Broadcast;

namespace Netst.ValueConverters
{
    public class NodeAdapterExtraParamToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            NodeInfo node = value as NodeInfo;
            if (node == null)
                return null;

            if (parameter == null)
                return null;

            string[] parameters = parameter.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            string v = node.GetExtraParam(parameters[0]);

            if (parameters.Length > 1)
                return string.Format(parameters[1], v);

            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
