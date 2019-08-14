using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Netst.NetstApi
{
    public class TcpListenerBackend : NetstListenerBackend
    {
        // IVars
        protected Timer SocketAcceptTimer;
        protected TcpListener Listener;

        // Properties
        public override string Protocol => "TCP";
        public override bool Active => Started;


        // Constructor
        public TcpListenerBackend(IPAddress address, ushort port)
        {
            Listener = new TcpListener(address, port);
            Address = address;
            Port = port;
            Listener.Start();
            Started = true;

            SocketAcceptTimer = new Timer(1000) {AutoReset = true};
            SocketAcceptTimer.Elapsed += SocketAcceptTimerOnElapsed;
        }

        // Event Handlers
        private void SocketAcceptTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Disposing)
                return;

            if (!Started)
                return;

            SocketAcceptTimer.Stop();

            TcpClient tcp;

            lock (Listener)
                tcp = Listener.AcceptTcpClient();

            var backend = new TcpClientBackend(tcp);

            Client c = new Client(backend);

            lock (Clients)
                Clients.Add(c);

            c.Start();

            SocketAcceptTimer.Start();
        }

        // Methods
        public override void Start()
        {
            Listener.Start();

            SocketAcceptTimer.Start();
            Started = true;
        }

        public override void Stop()
        {
            Started = false;

            SocketAcceptTimer.Stop();
            Listener.Stop();

            if (Listener != null)
                Listener.Stop();

            Listener = null;
        }

        public override void Dispose()
        {
            Disposing = true;

            Stop();
            Listener = null;

            lock (Clients)
            {
                foreach (Client c in Clients)
                    c?.Dispose();

                Clients.Clear();
            }
        }
    }
}
