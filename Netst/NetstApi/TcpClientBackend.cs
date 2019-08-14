using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Netst.NetstApi
{
    public class TcpClientBackend : NetstClientBackend
    {
        // IVars
        private readonly TcpClient _client;
        protected byte[] TxBuffer;
        protected Stream NetStream;
        protected Timer RxReadTimer;
        protected Timer TxWriteTimer;
        protected Thread RxReadThread;
        protected Thread TxWriteThread;
        protected bool AbortThreads;


        // Properties
        public override string Protocol => "TCP";
        public override bool Active => _client.Connected;


        // Constructor
        public TcpClientBackend(TcpClient client)
        {
            UsingTimers = UseTimers;
            _client = client;
            NetStream = _client.GetStream();
            NetStream.ReadTimeout = 500;
            NetStream.WriteTimeout = 1500;

            var endpoint = _client.Client.RemoteEndPoint as IPEndPoint;

            Address = endpoint?.Address;
            if (endpoint != null)
                Port = (ushort) (endpoint.Port);

            Initialize();
        }
        public TcpClientBackend(IPAddress address, ushort port)
        {
            _client = new TcpClient(address.ToString(), port);
            NetStream = _client.GetStream();

            NetStream.ReadTimeout = 500;
            NetStream.WriteTimeout = 1500;

            Address = address;
            Port = port;

            Initialize();
        }

        private void Initialize()
        {
            TxBuffer = new byte[TxBufferSize];
            Random rng = new Random();
            Parallel.For(0, TxBuffer.Length, i => { TxBuffer[i] = (byte)rng.Next(0, 256); });

            RateTimer = new Timer(500) { AutoReset = true };
            RateTimer.Elapsed += RateTimerOnElapsed;
            RateTimer.Start();

            if (UsingTimers)
            {
                RxReadTimer = new Timer(10) { AutoReset = true };
                RxReadTimer.Elapsed += RxReadTimerOnElapsed;

                TxWriteTimer = new Timer(10) { AutoReset = true };
                TxWriteTimer.Elapsed += TxWriteTimerOnElapsed;
            }
        }


        // Methods
        public override void Start()
        {
            Started = true;

            if (UsingTimers)
            {
                TxWriteTimer.Start();
                RxReadTimer.Start();
            }
            else
            {
                AbortThreads = false;

                TxWriteThread = new Thread(TxWriteThread_Start);
                TxWriteThread.Priority = ThreadPriority.BelowNormal;
                TxWriteThread.IsBackground = true;

                RxReadThread = new Thread(RxReadThread_Start);
                RxReadThread.Priority = ThreadPriority.BelowNormal;
                RxReadThread.IsBackground = true;

                TxWriteThread.Start();
                RxReadThread.Start();
            }
        }

        public override void Stop()
        {
            Started = false;

            if (UsingTimers)
            {
                RxReadTimer.Stop();
                TxWriteTimer.Stop();
            }
            else
            {
                AbortThreads = true;
                if (RxReadThread.IsAlive)
                    RxReadThread.Join();
                if (TxWriteThread.IsAlive)
                    TxWriteThread.Join();
            }

            if (_client.Connected)
                _client.Close();
        }

        // Event Handlers
        protected virtual void TxWriteTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!Started)
                return;

            TxWriteTimer.Stop();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds < 10)
            {
                sw.Restart();

                try
                {
                    NetStream.Write(TxBuffer, 0, TxBuffer.Length);
                    Txratecummulator += TxBuffer.Length;
                }
                catch (Exception)
                {
                    // Swallow
                }
            }

            TxWriteTimer.Start();
        }
        protected virtual void RxReadTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!Started)
                return;

            RxReadTimer.Stop();
            byte[] buffer = new byte[RxBufferSize];

            int read = 1;
            while (_client.Available > 0 && read >= 1)
            {
                try
                {
                    int available = _client.Available;
                    read = NetStream.Read(buffer, 0, Math.Min(available, buffer.Length));
                }
                catch (Exception)
                {
                    read = 0;
                }
                Rxratecummulator += read;
            }

            RxReadTimer.Start();
        }
        protected virtual void RxReadThread_Start()
        {
            if (!Started)
                return;

            byte[] buffer = new byte[RxBufferSize];

            int read;
            while (!AbortThreads)
            {
                while (_client.Available < 1)
                    Thread.Sleep(5);

                try
                {
                    int available = _client.Available;
                    read = NetStream.Read(buffer, 0, Math.Min(available, buffer.Length));
                }
                catch (Exception)
                {
                    read = 0;
                }

                Rxratecummulator += read;
            }
        }
        protected virtual void TxWriteThread_Start()
        {
            if (!Started)
                return;

            while (!AbortThreads)
            {
                try
                {
                    NetStream.Write(TxBuffer, 0, TxBuffer.Length);
                    Txratecummulator += TxBuffer.Length;
                }
                catch (Exception)
                {
                    // Swallow
                }
            }
        }
        public override void Dispose()
        {
            Stop();
            NetStream = null;
            _client?.Close();

            if (UsingTimers)
            {
                TxWriteTimer.Stop();
                TxWriteTimer.Dispose();
                RxReadTimer.Stop();
                RxReadTimer.Dispose();
            }

            RateTimer.Stop();
        }
    }
}
