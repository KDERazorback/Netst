using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Netst.NetstApi
{
    public class UdpClientBackend : NetstClientBackend
    {
        // IVars
        private readonly UdpClient _client;
        private bool _active;
        protected byte[] TxBuffer;
        protected Timer TxWriteTimer;
        protected Thread TxWriteThread;
        protected bool AbortThreads;
        private readonly IPEndPoint _endPoint;


        // Properties
        public override string Protocol => "UDP";
        public override bool Active => _active;
        protected bool IsStreamDataAvailable => _client.Available > 0;
        protected int StreamDataAvailable => _client.Available;


        // Constructor
        public UdpClientBackend(UdpClient client, IPEndPoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            UsingTimers = UseTimers;
            _client = client;

            Address = endpoint.Address;
            _endPoint = endpoint;
            Port = (ushort)(endpoint.Port);

            Initialize();
        }

        public UdpClientBackend(IPAddress address, ushort port)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            UsingTimers = UseTimers;
            Address = Address;
            _endPoint = new IPEndPoint(address, port);
            _client = new UdpClient(_endPoint);

            Initialize();
        }

        // Methods
        protected void Initialize()
        {
            TxBuffer = new byte[TxBufferSize];
            Random rng = new Random();
            Parallel.For(0, TxBuffer.Length, i => { TxBuffer[i] = (byte)rng.Next(0, 256); });

            RateTimer = new Timer(500) { AutoReset = true };
            RateTimer.Elapsed += RateTimerOnElapsed;
            RateTimer.Start();

            if (UsingTimers)
            {
                TxWriteTimer = new Timer(10) { AutoReset = true };
                TxWriteTimer.Elapsed += TxWriteTimerOnElapsed;
            }
        }
        internal void OnRx(byte[] data)
        {
            if (!Started)
                return;

            if (data == null || data.Length < 1)
                return;

            Rxratecummulator += data.Length;
        }
        public override void Start()
        {
            Started = true;

            if (UsingTimers)
                TxWriteTimer.Start();
            else
            {
                AbortThreads = false;

                TxWriteThread = new Thread(TxWriteThread_Start);
                TxWriteThread.Priority = ThreadPriority.BelowNormal;
                TxWriteThread.IsBackground = true;

                TxWriteThread.Start();
            }
        }
        public override void Stop()
        {
            Started = false;

            if (UsingTimers)
            {
                TxWriteTimer.Stop();
            }
            else
            {
                AbortThreads = true;
                if (TxWriteThread.IsAlive)
                    TxWriteThread.Join();
            }

            _client?.Close();
        }

        public override void Dispose()
        {
            Stop();

            TxBuffer = null;
            TxWriteThread = null;
            TxWriteTimer.Dispose();
        }


        // Event Handlers
        private void TxWriteTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
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
                    _client.Send(TxBuffer, TxBuffer.Length, _endPoint);
                    Txratecummulator += TxBuffer.Length;
                }
                catch (Exception)
                {
                    // Swallow
                }
            }

            TxWriteTimer.Start();
        }
        private void TxWriteThread_Start()
        {
            if (!Started)
                return;

            while (!AbortThreads)
            {
                try
                {
                    _client.Send(TxBuffer, TxBuffer.Length, _endPoint);
                    Txratecummulator += TxBuffer.Length;
                }
                catch (Exception)
                {
                    // Swallow
                }
            }
        }
    }
}
