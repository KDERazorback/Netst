using System;
using System.Net;
using System.Timers;

namespace Netst.NetstApi
{
    public class Server : IDisposable
    {
        // IVars
        private volatile float _txRate;
        private volatile float _rxRate;
        private volatile float _txmaxrate;
        private volatile float _rxmaxrate;
        private volatile float _peakBandwidth;
        private volatile float _usedBandwidth;
        private readonly CircularBuffer<float> _txBuffer; // Fills twice per second
        private readonly CircularBuffer<float> _rxBuffer; // Fills twice per second
        private readonly Timer _rateTimer;
        private bool _disposing;
        private int _xxBufferCount = 0;

        // Properties
        public NetstListenerBackend Backend { get; protected set; }
        public IPAddress Address => Backend.Address;
        public ushort Port => Backend.Port;
        public int ClientCount => Backend.ClientCount;
        public Client[] Clients => Backend.Clients.ToArray();
        public float OverallTx => _txRate;
        public float OverallRx => _rxRate;
        public float[] TxHistory
        {
            get
            {
                float[] output;

                lock (_txBuffer)
                    output = _txBuffer.ToArray();

                return output;
            }
        }
        public float[] RxHistory
        {
            get
            {
                float[] output;

                lock (_rxBuffer)
                    output = _rxBuffer.ToArray();

                return output;
            }
        }
        public bool Started => Backend.Started;
        public float UsedBandwidth => _usedBandwidth;
        public float PeakBandwidth => _peakBandwidth;
        public float PeakTx => _txmaxrate;
        public float PeakRx => _rxmaxrate;
        public int XxBufferCount => _xxBufferCount;

        // Indexers
        public Client this[int i] => Clients[i];

        // Constructors
        public Server(IPAddress address, ushort port, bool isUdp = false)
        {
            NetstListenerBackend backend;

            if (isUdp)
                backend = new UdpListenerBackend(address, port);
            else
                backend = new TcpListenerBackend(address, port);

            Backend = backend;

            _rxBuffer = new CircularBuffer<float>(2 * 60 * 5); // 5 minutes, twice entries per second
            _txBuffer = new CircularBuffer<float>(2 * 60 * 5); // 5 minutes, twice entries per second

            _rateTimer = new Timer(500) {AutoReset = true};
            _rateTimer.Elapsed += RateTimerOnElapsed;
            _rateTimer.Start();
        }

        // Event Handlers
        private void RateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_disposing)
                return;
            if (!Started)
                return;

            _rateTimer.Stop();

            float tx = 0;
            float rx = 0;

            lock (Clients)
            {
                for (int i = 0; i < Backend.Clients.Count; i++)
                {
                    Client c = Backend.Clients[i];

                    if (!c.IsStarted)
                    {
                        Backend.Clients.RemoveAt(i);
                        continue;
                    }

                    rx += c.Rx;
                    tx += c.Tx;
                }

                if (tx >= 1.0f && Backend.Clients.Count > 0)
                    tx = tx / Backend.Clients.Count;
                if (rx >= 1.0f && Backend.Clients.Count > 0)
                    rx = rx / Backend.Clients.Count;
            }

            if (tx >= _txmaxrate)
                _txmaxrate = tx;

            if (rx >= _rxmaxrate)
                _rxmaxrate = rx;

            _txRate = tx;
            _rxRate = rx;

            if (_peakBandwidth < tx + rx)
                _peakBandwidth = tx + rx;

            _usedBandwidth = tx + rx;
            
            lock (_txBuffer)
                _txBuffer.Insert(tx);

            lock (_rxBuffer)
                _rxBuffer.Insert(rx);

            if (XxBufferCount < _txBuffer.Capacity)
                _xxBufferCount++;

            _rateTimer.Start();
        }

        // Methods
        public void Start()
        {
            Backend.Start();
            _rateTimer.Start();
        }

        public void Stop()
        {
            Backend.Stop();
            _rateTimer.Stop();
        }

        public void Dispose()
        {
            _disposing = true;
            Backend?.Dispose();
        }

        public void AttachClient(Client c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));

            lock (Backend.Clients)
                Backend.Clients.Add(c);
        }
    }
}
