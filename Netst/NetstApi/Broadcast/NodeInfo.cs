using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Netst.NetstApi.Broadcast
{
    [DataContract]
    public class NodeInfo
    {
        private DateTime? _discoveryTime = DateTime.Now;
        private IPAddress _address;

        [DataMember] public string MachineName { get; set; }
        [DataMember] public OperatingSystem SystemInfo { get; set; }
        [DataMember] public string DnsName { get; set; }
        [DataMember] public ushort Port { get; set; }
        [DataMember] public string AppId { get; set; }
        [DataMember] public UInt32 FeaturesMask { get; set; }
        [DataMember] public string[][] ExtraParams { get; set; }

        [DataMember] public string AddressStr
        {
            get { return _address.ToString(); }
            set { _address = IPAddress.Parse(value); }
        }

        public bool IsPortUdp
        {
            get { return (FeaturesMask & 0b0000000000000001) > 0; }
            set
            {
                UInt32 mask = 0b0000000000000001;

                if (value)
                    FeaturesMask |= mask;
                else
                    FeaturesMask &= ~mask;
            }
        }
        public DateTime DiscoveredAt
        {
            get
            {
                if (_discoveryTime == null)
                    _discoveryTime = DateTime.Now;

                return _discoveryTime.Value;
            }
        }

        public IPAddress Address => _address;

        protected NodeInfo() { }

        public NodeInfo(Server parentServer)
        {
            OperatingSystem systeminfo;
            try
            {
                systeminfo = Environment.OSVersion;
            }
            catch (Exception)
            {
                systeminfo = new OperatingSystem(PlatformID.Win32NT, new Version(0, 0, 0, 0));
            }

            string hostname;
            try
            {
                hostname = Dns.GetHostEntry(parentServer.Address).HostName;
            }
            catch (Exception)
            {
                hostname = "";
            }

            _address = parentServer.Address;
            AppId = "NetStApi;0.0.1.5";
            DnsName = hostname;
            MachineName = Environment.MachineName;
            Port = parentServer.Port;
            SystemInfo = systeminfo;
            ExtraParams = new string[0][];

            IsPortUdp = string.Equals(parentServer.Backend.Protocol, "udp", StringComparison.OrdinalIgnoreCase);

            _discoveryTime = DateTime.Now;
        }

        public string GetExtraParam(string key)
        {
            foreach (string[] param in ExtraParams)
            {
                if (string.Equals(param[0], key, StringComparison.OrdinalIgnoreCase))
                    return param[1];
            }

            return "---";
        }


        public byte[] Serialize()
        {
            return Serialize(this);
        }

        public static byte[] Serialize(NodeInfo instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            byte[] data;
            using (MemoryStream mem = new MemoryStream())
            {
                XmlSerializer ser = new XmlSerializer(typeof(NodeInfo));

                using (GZipStream gz = new GZipStream(mem, CompressionLevel.Fastest, true))
                    ser.Serialize(gz, instance);

                data = mem.ToArray();
            }

            return data;
        }

        public static NodeInfo Deserialize(byte[] data)
        {
            if (data == null || data.Length < 1)
                throw new ArgumentNullException(nameof(data));

            NodeInfo node;
            using (MemoryStream mem = new MemoryStream(data, false))
            {
                XmlSerializer ser = new XmlSerializer(typeof(NodeInfo));
                MemoryStream mem2 = new MemoryStream();

                using (GZipStream stream = new GZipStream(mem, CompressionMode.Decompress))
                    stream.CopyTo(mem2);

                mem2.Seek(0, SeekOrigin.Begin);
                node = (NodeInfo)ser.Deserialize(mem2);
            }

            return node;
        }
    }
}
