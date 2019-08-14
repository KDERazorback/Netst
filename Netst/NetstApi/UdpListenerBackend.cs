using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Netst.NetstApi
{
    public class UdpListenerBackend : NetstListenerBackend
    {
        // IVars
        protected UdpClient Listener;
        protected Dictionary<IPEndPoint, Client> ClientMapping;
        protected IAsyncResult LastAsyncReceiveOp;


        // Properties
        public override string Protocol => "UDP";
        public override bool Active => Started;


        // Constructors
        public UdpListenerBackend(IPAddress address, ushort port)
        {
            ClientMapping = new Dictionary<IPEndPoint, Client>(96);
            Address = address;
            Port = port;
            Listener = new UdpClient(new IPEndPoint(address, port));
        }

        // Event Handlers
        private void Socket_OnReceive(IAsyncResult result)
        {
            IPEndPoint endpoint = null;
            byte[] data = Listener.EndReceive(result, ref endpoint);

            if (!Disposing && Started)
            {
                LastAsyncReceiveOp = Listener.BeginReceive(Socket_OnReceive, null);

                lock (Clients)
                {
                    Client c;
                    UdpClientBackend backend;

                    if (ClientMapping.TryGetValue(endpoint, out c))
                    {
                        backend = c.Backend as UdpClientBackend;
                        backend?.OnRx(data);
                    }
                    else
                    {
                        backend = new UdpClientBackend(Listener, endpoint);
                        c = new Client(backend);
                        Clients.Add(c);
                        ClientMapping.Add(endpoint, c);

                        backend.OnRx(data);
                    }
                }
            }
        }


        // Methods
        public override void Start()
        {
            LastAsyncReceiveOp = Listener.BeginReceive(Socket_OnReceive, null);
            Started = true;
        }

        public override void Stop()
        {
            if (LastAsyncReceiveOp != null)
            {
                IPEndPoint nop = null;
                Listener.EndReceive(LastAsyncReceiveOp, ref nop);
                LastAsyncReceiveOp = null;
            }

            Started = false;
        }

        public override void Dispose()
        {
            Stop();
            Listener = null;
            foreach (Client c in Clients)
                c.Dispose();

            ClientMapping.Clear();
            ClientMapping = null;

            Clients = null;
        }
    }
}
