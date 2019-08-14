using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Netst.NetstApi;

namespace Netst
{
    /// <summary>
    /// Lógica de interacción para ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window
    {
        public static readonly DependencyProperty AddressProperty = DependencyProperty.Register(
            "Address", typeof(IPAddress), typeof(ConnectWindow), new PropertyMetadata(null));

        public IPAddress Address
        {
            get { return (IPAddress) GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        public static readonly DependencyProperty PortProperty = DependencyProperty.Register(
            "Port", typeof(ushort), typeof(ConnectWindow), new PropertyMetadata(default(ushort)));

        public ushort Port
        {
            get { return (ushort) GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public static readonly DependencyProperty PreferUdpProperty = DependencyProperty.Register(
            "PreferUdp", typeof(bool), typeof(ConnectWindow), new PropertyMetadata(false));

        public bool PreferUdp
        {
            get { return (bool) GetValue(PreferUdpProperty); }
            set { SetValue(PreferUdpProperty, value); }
        }

        public static readonly DependencyProperty ConnectionTaskProperty = DependencyProperty.Register(
            "ConnectionTask", typeof(Task), typeof(ConnectWindow), new PropertyMetadata(default(Task)));

        public Task ConnectionTask
        {
            get { return (Task) GetValue(ConnectionTaskProperty); }
            set { SetValue(ConnectionTaskProperty, value); }
        }

        public bool Aborted { get; set; }

        public ConnectWindow()
        {
            InitializeComponent();
        }

        public void Start()
        {
            IPAddress add = Address;
            ushort prt = Port;
            bool udp = PreferUdp;

            ConnectionTask = new Task(() =>
            {
                ConnectWindow parent = this;
                Client c = new Client(add, prt, udp);
                c.Start();

                if (parent.Aborted)
                {
                    c.Stop();
                    return;
                }

                Settings.Volatile.ActiveServer.AttachClient(c);
            });
            ConnectionTask.ContinueWith(task =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (Aborted) return;

                    if (task.IsFaulted)
                    {
                        MessageBox.Show(
                            "An error occured while trying to connect to the specified address.\n\n" +
                            Address.ToString() +
                            " : " + (PreferUdp
                                ? "UDP "
                                : "TCP ") + Port + "\n\n" + task.Exception?.ToString(), "Connection error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    this.Close();
                });
            });
            ConnectionTask.Start();
        }

        public static ConnectWindow ShowAndConnect(IPAddress address, ushort port, bool preferUdp)
        {
            ConnectWindow wnd = new ConnectWindow();
            wnd.Address = address;
            wnd.Port = port;
            wnd.PreferUdp = preferUdp;

            wnd.Show();
            wnd.Start();

            return wnd;
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            Aborted = true;
            Close();
        }
    }
}
