using System;
using System.Net;

namespace Netst.NetstApi
{
    public class Client : IDisposable
    {
        // IVars

        // Properties
        public readonly NetstClientBackend Backend;
        public IPAddress Address => Backend.Address;
        public ushort Port => Backend.Port;
        public string Protocol => Backend.Protocol;
        public bool IsStarted => Backend.Started && Backend.Active;
        public float Rx => Backend.RxRate;
        public float Tx => Backend.TxRate;
        public bool Outgoing { get; protected set; }

        // Constructor
        public Client(NetstClientBackend backend)
        {
            Outgoing = false;
            Backend = backend;
        }
        public Client(IPAddress address, ushort port, bool udpBackend = false)
        {
            NetstClientBackend backend;
            Outgoing = true;

            if (udpBackend)
                backend = new UdpClientBackend(address, port);
            else
                backend = new TcpClientBackend(address, port);

            Backend = backend;
        }

        // Event Handlers


        // Methods
        public void Start()
        {
            Backend.Start();
        }

        public void Stop()
        {
            Backend.Stop();
        }

        public void Dispose()
        {
            Backend?.Dispose();
        }
    }
}
