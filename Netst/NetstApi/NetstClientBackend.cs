using System;
using System.Net;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Netst.NetstApi
{
    public abstract class NetstClientBackend : IDisposable
    {

        // IVars
        protected volatile float Txrate;
        protected volatile float Txratecummulator;
        protected volatile float Rxratecummulator;
        protected volatile float Rxrate;
        protected volatile float Txmaxrate;
        protected volatile float Rxmaxrate;
        public static bool UseTimers { get; set; } = false;
        public bool UsingTimers { get; protected set; }
        protected Timer RateTimer;
        protected bool Disposing;


        // Properties
        public float TxRate => Txrate;
        public float RxRate => Rxrate;
        public bool Started { get; protected set; }
        public IPAddress Address { get; protected set; }
        public ushort Port { get; protected set; }
        public abstract string Protocol { get; }
        public abstract bool Active { get; }
        public int TxBufferSize { get; set; } = 4096; // 4 KiB
        public int RxBufferSize { get; set; } = 4096; // 4 KiB


        // Methods
        public abstract void Start();
        public abstract void Stop();
        public abstract void Dispose();


        // Event Handlers
        protected virtual void RateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Disposing)
                return;

            if (!Started)
                return;

            Txrate = Txratecummulator * (float)(1000 / RateTimer.Interval) * 8;
            Rxrate = Rxratecummulator * (float)(1000 / RateTimer.Interval) * 8;

            if (Txrate >= Txmaxrate)
                Txmaxrate = Txrate;

            if (Rxrate >= Rxmaxrate)
                Rxmaxrate = Rxrate;

            Rxratecummulator = 0;
            Txratecummulator = 0;
        }
    }
}
