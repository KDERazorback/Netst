using System;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Netst.NetstApi.Broadcast
{
    public class Announcer
    {
        public const ushort DefaultPort = 14855;

        protected NodeInfo MachineData;
        protected byte[] MachineDataBuffer;
        protected Timer PingTimer;
        protected UdpClient UdpSocket;

        public bool Started { get; protected set; }
        public Server BoundServer { get; protected set; }
        public ushort Port { get; protected set; }
        public TimeSpan PingInterval { get; protected set; }

        public delegate void MachineDataPreparedDelegate(Announcer announcer, Server server, ref NodeInfo info);
        public event MachineDataPreparedDelegate MachineDataPrepared;

        public Announcer(Server server) : this(server, DefaultPort) { }

        public Announcer(Server server, ushort port) : this(server, port, new TimeSpan(0, 0, 0, 7)) { }

        public Announcer(Server server, ushort port, TimeSpan interval)
        {
            BoundServer = server;
            Port = port;
            PingInterval = interval;

            PingTimer = new Timer(PingInterval.TotalMilliseconds);
            PingTimer.AutoReset = false;
            PingTimer.Elapsed += PingTimerOnElapsed;

            UdpSocket = new UdpClient();
            UdpSocket.MulticastLoopback = false;
            UdpSocket.EnableBroadcast = true;
        }

        public void Start()
        {
            if (Started)
                throw new InvalidOperationException("Announcer already started.");

            Started = true;
            PingTimer.Start();
        }

        public void Stop()
        {
            Started = false;
            PingTimer.Stop();
            UdpSocket.Close();
            MachineData = null;
            MachineDataBuffer = null;
        }

        private void PingTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!Started)
                return;

            SendPing();

            PingTimer.Start();
        }

        public void ForcePing()
        {
            SendPing();
        }

        protected virtual void SendPing()
        {
            if (MachineData == null || (DateTime.Now - MachineData.DiscoveredAt).TotalSeconds > 120)
            {
                // Update machine data
                MachineData = new NodeInfo(BoundServer);
                OnMachineDataPrepared(ref MachineData);
                MachineDataBuffer = MachineData.Serialize();
            }

            UdpSocket.Send(MachineDataBuffer, MachineDataBuffer.Length, new IPEndPoint(IPAddress.Broadcast, Port));
        }

        protected virtual void OnMachineDataPrepared(ref NodeInfo info)
        {
            MachineDataPrepared?.Invoke(this, this.BoundServer, ref info);
        }
    }
}
