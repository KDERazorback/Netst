using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Netst
{
    public class NetstNetworkAdapter
    {
        // IVars
        private readonly NetworkInterface _interface;

        // Properties
        public bool IsActive => _interface.OperationalStatus == OperationalStatus.Up;
        public string SpeedString => SpeedToString(_interface.Speed, "bps", false);
        public long Speed => _interface.Speed;
        public OperationalStatus Status => _interface.OperationalStatus;
        public string IdHandle => _interface.Id;
        public string Name => _interface.Name;
        public NetworkInterfaceType Type => _interface.NetworkInterfaceType;
        public bool IsEthernet => _interface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                                  _interface.NetworkInterfaceType == NetworkInterfaceType.Ethernet3Megabit ||
                                  _interface.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT ||
                                  _interface.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet;
        public bool IsRadio => _interface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                               _interface.NetworkInterfaceType == NetworkInterfaceType.Wman ||
                               _interface.NetworkInterfaceType == NetworkInterfaceType.Wwanpp ||
                               _interface.NetworkInterfaceType == NetworkInterfaceType.Wwanpp2;
        public bool IsFiber => _interface.NetworkInterfaceType == NetworkInterfaceType.Fddi ||
                               _interface.NetworkInterfaceType == NetworkInterfaceType.Ethernet3Megabit;

        public bool RxOnly => _interface.IsReceiveOnly;
        public bool MulticastSupported => _interface.SupportsMulticast;
        public bool TxSupported => !MulticastSupported;
        public NetworkInterface Adapter => _interface;

        public IPAddress Ipv4Address
        {
            get
            {
                IPAddress output;
                output = _interface.GetIPProperties()?.UnicastAddresses.Where(i => i.Address.AddressFamily == AddressFamily.InterNetwork)?.First().Address.MapToIPv4();

                return output;
            }
        }

        public IPAddress Ipv6Address
        {
            get
            {
                IPAddress output;
                output = _interface.GetIPProperties()?.UnicastAddresses.Where(i => i.Address.AddressFamily == AddressFamily.InterNetworkV6)?.First().Address.MapToIPv4();


                return output;
            }
        }

        public string MacAddress
        {
            get { return _interface.GetPhysicalAddress()?.ToString(); }
        }


        // Constructor
        public NetstNetworkAdapter(NetworkInterface i)
        {
            _interface = i;
        }

        // Static Methods
        public static string SpeedToString(long value, string unit, bool powerOfTwoBased = true)
        {
            return SpeedToString((float) value, unit, powerOfTwoBased);
        }
        public static string SpeedToString(float value, string unit, bool powerOfTwoBased = true)
        {
            if (value < 1)
                return "0 " + unit;

            int max = powerOfTwoBased ? 1024 : 1000;
            string[] multiples = { "", "k", "m", "g", "t" };
            int pointer = 0;
            while (value >= max)
            {
                if (pointer >= multiples.Length - 1)
                    break;

                value /= max;
                pointer++;
            }

            if (value % 100 > 0.9f)
                return value.ToString("N2") + " " + multiples[pointer] + unit;

            return value.ToString("N0") + " " + multiples[pointer] + unit;
        }
    }
}
