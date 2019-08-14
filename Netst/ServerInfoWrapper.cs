using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Timers;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using Netst.NetstApi;
using Netst.NetstApi.Broadcast;

namespace Netst
{
    public class ServerInfoWrapper : UIElement, IDisposable
    {
        private Server _server;
        private readonly Timer _updateTimer;
        private readonly Stopwatch _clientUpdateSw;

        public Server Instance => _server;

        public static readonly DependencyProperty OverallRxProperty = DependencyProperty.Register(
            "OverallRx", typeof(float), typeof(ServerInfoWrapper), new PropertyMetadata(default(float)));

        public float OverallRx
        {
            get { return (float) GetValue(OverallRxProperty); }
            set { SetValue(OverallRxProperty, value); }
        }

        public static readonly DependencyProperty OverallTxProperty = DependencyProperty.Register(
            "OverallTx", typeof(float), typeof(ServerInfoWrapper), new PropertyMetadata(default(float)));

        public float OverallTx
        {
            get { return (float) GetValue(OverallTxProperty); }
            set { SetValue(OverallTxProperty, value); }
        }

        public static readonly DependencyProperty ProtocolNameProperty = DependencyProperty.Register(
            "ProtocolName", typeof(string), typeof(ServerInfoWrapper), new PropertyMetadata("Unknown"));

        public string ProtocolName
        {
            get { return (string) GetValue(ProtocolNameProperty); }
            set { SetValue(ProtocolNameProperty, value); }
        }

        public static readonly DependencyProperty PortProperty = DependencyProperty.Register(
            "Port", typeof(ushort), typeof(ServerInfoWrapper), new PropertyMetadata(default(ushort)));

        public ushort Port
        {
            get { return (ushort) GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public static readonly DependencyProperty ClientCountProperty = DependencyProperty.Register(
            "ClientCount", typeof(int), typeof(ServerInfoWrapper), new PropertyMetadata(default(int)));

        public int ClientCount
        {
            get { return (int) GetValue(ClientCountProperty); }
            set { SetValue(ClientCountProperty, value); }
        }

        public static readonly DependencyProperty ClientsProperty = DependencyProperty.Register(
            "Clients", typeof(ObservableCollection<Client>), typeof(ServerInfoWrapper), new PropertyMetadata(new ObservableCollection<Client>()));

        public ObservableCollection<Client> Clients
        {
            get { return (ObservableCollection<Client>) GetValue(ClientsProperty); }
            set { SetValue(ClientsProperty, value); }
        }

        public static readonly DependencyProperty IsStartedProperty = DependencyProperty.Register(
            "IsStarted", typeof(bool), typeof(ServerInfoWrapper), new PropertyMetadata(default(bool)));

        public bool IsStarted
        {
            get { return (bool) GetValue(IsStartedProperty); }
            set { SetValue(IsStartedProperty, value); }
        }

        public static readonly DependencyProperty TxHistoryProperty = DependencyProperty.Register(
            "TxHistory", typeof(float[]), typeof(ServerInfoWrapper), new PropertyMetadata(default(float[])));

        public float[] TxHistory
        {
            get { return (float[]) GetValue(TxHistoryProperty); }
            set { SetValue(TxHistoryProperty, value); }
        }

        public static readonly DependencyProperty RxHistoryProperty = DependencyProperty.Register(
            "RxHistory", typeof(float[]), typeof(ServerInfoWrapper), new PropertyMetadata(default(float[])));

        public float[] RxHistory
        {
            get { return (float[]) GetValue(RxHistoryProperty); }
            set { SetValue(RxHistoryProperty, value); }
        }

        public static readonly DependencyProperty IpAddressProperty = DependencyProperty.Register(
            "IpAddress", typeof(IPAddress), typeof(ServerInfoWrapper), new PropertyMetadata(default(IPAddress)));

        public static readonly DependencyProperty BandwidthProperty = DependencyProperty.Register(
            "Bandwidth", typeof(float), typeof(ServerInfoWrapper), new PropertyMetadata(default(float)));

        public static readonly DependencyProperty PeakTxProperty = DependencyProperty.Register(
            "PeakTx", typeof(float), typeof(ServerInfoWrapper), new PropertyMetadata(default(float)));

        public float PeakTx
        {
            get { return (float) GetValue(PeakTxProperty); }
            set { SetValue(PeakTxProperty, value); }
        }

        public static readonly DependencyProperty PeakRxProperty = DependencyProperty.Register(
            "PeakRx", typeof(float), typeof(ServerInfoWrapper), new PropertyMetadata(default(float)));

        public float PeakRx
        {
            get { return (float) GetValue(PeakRxProperty); }
            set { SetValue(PeakRxProperty, value); }
        }

        public float Bandwidth
        {
            get { return (float) GetValue(BandwidthProperty); }
            set { SetValue(BandwidthProperty, value); }
        }

        public static readonly DependencyProperty UsedBandwidthProperty = DependencyProperty.Register(
            "UsedBandwidth", typeof(float), typeof(ServerInfoWrapper), new PropertyMetadata(default(float)));

        public float UsedBandwidth
        {
            get { return (float) GetValue(UsedBandwidthProperty); }
            set { SetValue(UsedBandwidthProperty, value); }
        }

        public static readonly DependencyProperty TxHistorySerieProperty = DependencyProperty.Register(
            "TxHistorySerie", typeof(LineSeries), typeof(ServerInfoWrapper), new PropertyMetadata(null));

        public LineSeries TxHistorySerie
        {
            get { return (LineSeries) GetValue(TxHistorySerieProperty); }
            set { SetValue(TxHistorySerieProperty, value); }
        }

        public static readonly DependencyProperty RxHistorySerieProperty = DependencyProperty.Register(
            "RxHistorySerie", typeof(LineSeries), typeof(ServerInfoWrapper), new PropertyMetadata(null));

        public LineSeries RxHistorySerie
        {
            get { return (LineSeries) GetValue(RxHistorySerieProperty); }
            set { SetValue(RxHistorySerieProperty, value); }
        }

        public static readonly DependencyProperty TxHistorySeriesProperty = DependencyProperty.Register(
            "TxHistorySeries", typeof(SeriesCollection), typeof(ServerInfoWrapper), new PropertyMetadata(null));

        public SeriesCollection TxHistorySeries
        {
            get { return (SeriesCollection) GetValue(TxHistorySeriesProperty); }
            set { SetValue(TxHistorySeriesProperty, value); }
        }

        public static readonly DependencyProperty RxHistorySeriesProperty = DependencyProperty.Register(
            "RxHistorySeries", typeof(SeriesCollection), typeof(ServerInfoWrapper), new PropertyMetadata(null));

        public SeriesCollection RxHistorySeries
        {
            get { return (SeriesCollection) GetValue(RxHistorySeriesProperty); }
            set { SetValue(RxHistorySeriesProperty, value); }
        }

        public static readonly DependencyProperty RxHistoryCountProperty = DependencyProperty.Register(
            "RxHistoryCount", typeof(int), typeof(ServerInfoWrapper), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty XxBufferCountProperty = DependencyProperty.Register(
            "XxBufferCount", typeof(int), typeof(ServerInfoWrapper), new PropertyMetadata(0));

        public int XxBufferCount
        {
            get { return (int) GetValue(XxBufferCountProperty); }
            set { SetValue(XxBufferCountProperty, value); }
        }

        public static readonly DependencyProperty AutoDiscoveryEnabledProperty = DependencyProperty.Register(
            "AutoDiscoveryEnabled", typeof(bool), typeof(ServerInfoWrapper), new PropertyMetadata(false));

        public static readonly DependencyProperty DiscoveredClientsProperty = DependencyProperty.Register(
            "DiscoveredClients", typeof(ObservableCollection<NodeInfo>), typeof(ServerInfoWrapper), new PropertyMetadata(new ObservableCollection<NodeInfo>()));

        public ObservableCollection<NodeInfo> DiscoveredClients
        {
            get { return (ObservableCollection<NodeInfo>) GetValue(DiscoveredClientsProperty); }
            set { SetValue(DiscoveredClientsProperty, value); }
        }

        public bool AutoDiscoveryEnabled
        {
            get { return (bool) GetValue(AutoDiscoveryEnabledProperty); }
            set { SetValue(AutoDiscoveryEnabledProperty, value); }
        }

        public IPAddress IpAddress
        {
            get { return (IPAddress) GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
        }

        public ServerInfoWrapper() {  }

        public ServerInfoWrapper(Server instance)
        {
            _server = instance;
            ProtocolName = instance.Backend.Protocol;
            Port = instance.Backend.Port;
            IpAddress = instance.Address;

            TxHistorySerie = new LineSeries() { Name = "Tx" };
            RxHistorySerie = new LineSeries() { Name = "Rx" };
            TxHistorySerie.Values = new ChartValues<float>();
            RxHistorySerie.Values = new ChartValues<float>();
            TxHistorySeries = new SeriesCollection() { TxHistorySerie };
            RxHistorySeries = new SeriesCollection() { RxHistorySerie };

            Netst.Settings.Volatile.ClientDiscovered += RemoteClientDiscovered;

            _clientUpdateSw = new Stopwatch();

            _updateTimer = new Timer(250);
            _updateTimer.AutoReset = true;
            _updateTimer.Elapsed += UpdateTimerOnElapsed;
            _updateTimer.Start();

            _clientUpdateSw.Start();
        }

        private void RemoteClientDiscovered(Tracker tracker, IPEndPoint endpoint, NodeInfo info)
        {
            if (!CheckAccess())
            {
                Dispatcher.InvokeAsync(() => { RemoteClientDiscovered(tracker, endpoint, info); });
                return;
            }

            lock (DiscoveredClients)
            {
                foreach (NodeInfo i in DiscoveredClients)
                    if (string.Equals(i.AddressStr, info.AddressStr) && i.Port == info.Port)
                    {
                        DiscoveredClients.Remove(i);
                        break;
                    }

                DiscoveredClients.Add(info);
            }
        }

        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _updateTimer.Stop();

            if (!Settings.Volatile.PrimaryDispatcher.CheckAccess())
            {
                try
                {
                    Settings.Volatile.PrimaryDispatcher.InvokeAsync(UpdateMembers).GetAwaiter().OnCompleted(() => {
                        _updateTimer.Start();
                    });
                }
                catch (Exception)
                {
                    // Swallow
                }
            }
            else
                UpdateMembers();

            //_updateTimer.Start();
        }

        public void UpdateMembers()
        {
            ClientCount = _server.ClientCount;
            OverallRx = _server.OverallRx;
            OverallTx = _server.OverallTx;
            Bandwidth = _server.PeakBandwidth;
            UsedBandwidth = _server.UsedBandwidth;
            PeakTx = _server.PeakTx;
            PeakRx = _server.PeakRx;

            IsStarted = _server.Started;

            AutoDiscoveryEnabled = Netst.Settings.Persistent.TrackServers;

            lock (DiscoveredClients)
            {
                for (int i = DiscoveredClients.Count - 1; i >= 0; i--)
                    if ((DateTime.Now - DiscoveredClients[i].DiscoveredAt).TotalSeconds > 30)
                        DiscoveredClients.RemoveAt(i);
            }


            if (_clientUpdateSw.ElapsedMilliseconds > 1000)
            {
                XxBufferCount = _server.XxBufferCount;
                TxHistory = _server.TxHistory;
                RxHistory = _server.RxHistory;

                Client[] cs = _server.Clients;
                Clients.Clear();

                foreach (Client c in cs)
                    Clients.Add(c);

                TxHistorySerie.Values.Clear();
                RxHistorySerie.Values.Clear();

                int pointsInChart = 120; // 60 seconds

                for (int i = Math.Min(XxBufferCount, pointsInChart) - 1; i >= 0; i--)
                    TxHistorySerie.Values.Add(TxHistory[i] / 1000 / 1000);

                for (int i = Math.Min(XxBufferCount, pointsInChart) - 1; i >= 0; i--)
                    RxHistorySerie.Values.Add(RxHistory[i] / 1000 / 1000);

                _clientUpdateSw.Restart();
            }
        }

        public void Dispose()
        {
            _server?.Dispose();
            _updateTimer?.Dispose();
            _clientUpdateSw.Stop();
        }
    }
}
