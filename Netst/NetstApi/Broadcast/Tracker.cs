using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Netst.NetstApi.Broadcast
{
    public class Tracker
    {
        protected UdpClient UdpSocket;
        protected Thread PollThread;
        protected bool Aborting;

        public ushort Port { get; protected set; }
        public bool Started => PollThread?.IsAlive ?? false;

        public delegate void MessageReceivedDelegate(Tracker tracker, TrackerMessageReceivedEventArgs e);

        public event MessageReceivedDelegate InvalidMessageReceived;
        public event MessageReceivedDelegate MessageReceived;

        public Tracker() :this(Announcer.DefaultPort) { }
        public Tracker(ushort port)
        {
            Port = port;

            UdpSocket = new UdpClient(port);
            UdpSocket.EnableBroadcast = true;
        }

        public void Start()
        {
            if (Started)
                throw new InvalidOperationException("Tracker is already started.");

            ForgeThread();
            PollThread.Start();
        }

        public void Abort()
        {
            Aborting = true;
            PollThread.Join(10000);
            try
            {
                if (Started)
                    PollThread.Abort();
            }
            catch (Exception e)
            {
                // Swallow
            }
            UdpSocket.Close();
            PollThread = null;
        }

        private void PollThread_Start()
        {
            while (!Aborting)
            {
                while (UdpSocket.Available <= 0 && !Aborting)
                    Thread.Sleep(500);

                while (UdpSocket.Available > 0 && !Aborting)
                {
                    IPEndPoint endPoint = null;
                    byte[] packet = UdpSocket.Receive(ref endPoint);

                    if (packet != null && packet.Length > 0)
                    {
                        try
                        {
                            NodeInfo info = NodeInfo.Deserialize(packet);
                            OnMessageReceived(endPoint, packet, info);
                        }
                        catch (Exception e)
                        {
                            OnInvalidMessageReceived(endPoint, packet, e);
                        }
                    }
                }
            }
        }

        protected void ForgeThread()
        {
            PollThread = new Thread(PollThread_Start);
            PollThread.IsBackground = true;
            PollThread.Priority = ThreadPriority.Lowest;
        }

        protected virtual void OnInvalidMessageReceived(IPEndPoint endpoint, byte[] data, Exception exception)
        {
            InvalidMessageReceived?.Invoke(this, new TrackerMessageReceivedEventArgs(endpoint, data, exception));
        }

        protected virtual void OnMessageReceived(IPEndPoint endpoint, byte[] data, NodeInfo info)
        {
            MessageReceived?.Invoke(this, new TrackerMessageReceivedEventArgs(endpoint, data, info));
        }
    }
}
