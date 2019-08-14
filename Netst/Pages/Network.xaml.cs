using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Netst.NetstApi;
using Netst.NetstApi.Broadcast;

namespace Netst.Pages
{
    /// <summary>
    /// Lógica de interacción para NetworkAdapterView.xaml
    /// </summary>
    public partial class Network : Page
    {
        public static readonly DependencyProperty VolatileReferenceProperty = DependencyProperty.Register(
            "VolatileReference", typeof(VolatileSettings), typeof(Network), new PropertyMetadata(Netst.Settings.Volatile));

        public VolatileSettings VolatileReference
        {
            get { return (VolatileSettings) GetValue(VolatileReferenceProperty); }
            set { SetValue(VolatileReferenceProperty, value); }
        }

        public static readonly DependencyProperty SelectedServerPortStringProperty = DependencyProperty.Register(
            "SelectedServerPortString", typeof(string), typeof(Network), new PropertyMetadata("8544"));

        public string SelectedServerPortString
        {
            get { return (string) GetValue(SelectedServerPortStringProperty); }
            set { SetValue(SelectedServerPortStringProperty, value); }
        }

        public static readonly DependencyProperty ActiveServerProperty = DependencyProperty.Register(
            "ActiveServer", typeof(ServerInfoWrapper), typeof(Network), new PropertyMetadata(null));

        public ServerInfoWrapper ActiveServer
        {
            get { return (ServerInfoWrapper) GetValue(ActiveServerProperty); }
            set { SetValue(ActiveServerProperty, value); }
        }

        public static readonly DependencyProperty TestRunningProperty = DependencyProperty.Register(
            "TestRunning", typeof(bool), typeof(Network), new PropertyMetadata(false));

        public bool TestRunning
        {
            get { return (bool)GetValue(TestRunningProperty); }
            set { SetValue(TestRunningProperty, value); }
        }

        public static readonly DependencyProperty ClientConnectAddressStrProperty = DependencyProperty.Register(
            "ClientConnectAddressStr", typeof(string), typeof(Network), new PropertyMetadata(null));

        public string ClientConnectAddressStr
        {
            get { return (string) GetValue(ClientConnectAddressStrProperty); }
            set { SetValue(ClientConnectAddressStrProperty, value); }
        }

        public static readonly DependencyProperty ClientConnectPortStrProperty = DependencyProperty.Register(
            "ClientConnectPortStr", typeof(string), typeof(Network), new PropertyMetadata(null));

        public string ClientConnectPortStr
        {
            get { return (string) GetValue(ClientConnectPortStrProperty); }
            set { SetValue(ClientConnectPortStrProperty, value); }
        }

        public static readonly DependencyProperty TxSeriesCollectionProperty = DependencyProperty.Register(
            "TxSeriesCollection", typeof(SeriesCollection), typeof(Network), new PropertyMetadata(null));

        public static readonly DependencyProperty GaugesShowDetailedProperty = DependencyProperty.Register(
            "GaugesShowDetailed", typeof(bool), typeof(Network), new PropertyMetadata(false));

        public bool GaugesShowDetailed
        {
            get { return (bool) GetValue(GaugesShowDetailedProperty); }
            set { SetValue(GaugesShowDetailedProperty, value); }
        }

        public ICommand ClientDisconnectClickHandler { get; set; }
        public ICommand NodeConnectClickHandler { get; set; }

        public Network()
        {
            InitializeComponent();

            ClientDisconnectClickHandler = new RelayCommand<Client>(ClientDisconnect_OnClick);
            NodeConnectClickHandler = new RelayCommand<NodeInfo>(NodeConnect_OnClick);
        }

        private void NodeConnect_OnClick(NodeInfo nodeInfo)
        {
            ClientConnectAddressStr = nodeInfo.Address.ToString();
            ClientConnectPortStr = nodeInfo.Port.ToString();

            ClientConnect_OnClick(null, null);
        }

        private void ClientDisconnect_OnClick(Client client)
        {
            client.Stop();
        }

        private void ServerStartStop_OnClick(object sender, RoutedEventArgs e)
        {
            if (Netst.Settings.Volatile.TestRunning)
            {
                // Stop tests
                Netst.Settings.Volatile.ActiveAnnouncer?.Stop();
                Netst.Settings.Volatile.ActiveTracker?.Abort();
                Netst.Settings.Volatile.TestRunning = false;
                ActiveServer.Instance.Stop();
                Netst.Settings.Volatile.ActiveServer = null;
                TestRunning = false;
            }
            else
            {
                // Start tests
                if (Netst.Settings.Volatile.SelectedTestAdapter == null)
                {
                    MessageBox.Show("No Network Interface selected for testing.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (!Netst.Settings.Volatile.SelectedTestAdapter.IsActive)
                {
                    MessageBox.Show("The selected NetworkInterface is not active.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                int port;
                if (!int.TryParse(SelectedServerPortString, out port) || port <= 30 || port >= ushort.MaxValue)
                {
                    MessageBox.Show("Invalid port number specified.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                try
                {
                    ServerInfoWrapper wrapper = new ServerInfoWrapper(new Server(Netst.Settings.Volatile.SelectedTestAdapter.Ipv4Address, (ushort)port, Netst.Settings.Volatile.PreferUdp));
                    ActiveServer = wrapper;
                    Netst.Settings.Volatile.ActiveServer = ActiveServer.Instance;
                    ActiveServer.Instance.Start();
                    Netst.Settings.Volatile.TestRunning = true;
                    ClientConnectPortStr = port.ToString();
                    TestRunning = true;
                    if (Netst.Settings.Persistent.AnnounceServer)
                        AnnouncerStartWindow.ShowAndStart();
                    Netst.Settings.Volatile.TryAttachTracker();
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Failed to start the server on the specified Address and Port.\n\n" + exception.Message, "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    throw;
                }
                
            }
        }

        private async void ClientConnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (ActiveServer == null || !ActiveServer.IsStarted)
            {
                MessageBox.Show(
                    "The local server must be started before making any new connection.", "Server not started",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IPAddress address;
            ushort port;
            try
            {
                if (!IPAddress.TryParse(ClientConnectAddressStr, out address))
                {
                    Task<IPAddress> t = new Task<IPAddress>(() => Dns.GetHostAddresses(ClientConnectAddressStr).First());
                    await t;
                    address = t.Result;
                }

                if (address == null)
                    throw new InvalidDataException("Cannot resolve the specified hostname, or its an invalid IP address.");
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Cannot parse the destination address provided.\nIf the address is a domain name, it could be that the DNS servers cannot be reached.\n\n" +
                    exception.GetType().Name + "\n" + exception.Message, "Cannot get remote EndPoint",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                port = ushort.Parse(ClientConnectPortStr);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Cannot parse the specified port number.\nIt must be in the range of 30 to 65535.\n" +
                    exception.GetType().Name + "\n" + exception.Message, "Cannot get remote EndPoint",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                ConnectWindow.ShowAndConnect(address, port, Netst.Settings.Volatile.PreferUdp);
                ClientConnectAddressStr = "";
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Cannot connect to the specified address.\n\n" +
                    exception.GetType().Name + "\n" + exception.Message, "Cannot get remote EndPoint",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void Page_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Netst.Settings.Volatile.ActiveServer != null)
                ActiveServer = new ServerInfoWrapper(Netst.Settings.Volatile.ActiveServer);

            TestRunning = Netst.Settings.Volatile.TestRunning;
        }

        private void IpLabel_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null)
                return;

            ClientConnectAddressStr = lbl.Content.ToString();
        }

        private void GaugeArea_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            GaugesShowDetailed = !GaugesShowDetailed;
            e.Handled = true;
        }
    }
}
