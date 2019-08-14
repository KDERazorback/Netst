using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Netst.Pages
{
    /// <summary>
    /// Lógica de interacción para Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public static readonly DependencyProperty CpuCountProperty = DependencyProperty.Register(
            "CpuCount", typeof(int), typeof(Home), new PropertyMetadata(0));

        public int CpuCount
        {
            get { return (int) GetValue(CpuCountProperty); }
            set { SetValue(CpuCountProperty, value); }
        }

        public static readonly DependencyProperty AdapterCountProperty = DependencyProperty.Register(
            "AdapterCount", typeof(int), typeof(Home), new PropertyMetadata(0));

        public int AdapterCount
        {
            get { return (int) GetValue(AdapterCountProperty); }
            set { SetValue(AdapterCountProperty, value); }
        }

        public static readonly DependencyProperty SystemNameProperty = DependencyProperty.Register(
            "SystemName", typeof(string), typeof(Home), new PropertyMetadata("---"));

        public string SystemName
        {
            get { return (string) GetValue(SystemNameProperty); }
            set { SetValue(SystemNameProperty, value); }
        }

        public static readonly DependencyProperty DomainNameProperty = DependencyProperty.Register(
            "DomainName", typeof(string), typeof(Home), new PropertyMetadata("---"));

        public string DomainName
        {
            get { return (string) GetValue(DomainNameProperty); }
            set { SetValue(DomainNameProperty, value); }
        }

        public static readonly DependencyProperty AdapterListProperty = DependencyProperty.Register(
            "AdapterList", typeof(NetstNetworkAdapter[]), typeof(Home), new PropertyMetadata(null));

        public NetstNetworkAdapter[] AdapterList
        {
            get { return (NetstNetworkAdapter[]) GetValue(AdapterListProperty); }
            set { SetValue(AdapterListProperty, value); }
        }

        public static readonly DependencyProperty SelectedAdapterProperty = DependencyProperty.Register(
            "SelectedAdapter", typeof(NetstNetworkAdapter), typeof(Home), new PropertyMetadata(null));

        public NetstNetworkAdapter SelectedAdapter
        {
            get { return (NetstNetworkAdapter) GetValue(SelectedAdapterProperty); }
            set { SetValue(SelectedAdapterProperty, value); }
        }

        public Home()
        {
            InitializeComponent();
        }

        private void Home_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            SystemName = string.IsNullOrWhiteSpace(Environment.MachineName) ? "---" : Environment.MachineName;
            CpuCount = Environment.ProcessorCount;
            DomainName = string.IsNullOrWhiteSpace(Environment.UserDomainName)
                ? "--- No domain ---"
                : Environment.UserDomainName;

            List<NetstNetworkAdapter> interfaceNames = new List<NetstNetworkAdapter>();

            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface i in interfaces)
                    interfaceNames.Add(new NetstNetworkAdapter(i));
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Cannot get network adapter information.\n\n" + exception.GetType().FullName + "\n" +
                    exception.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            AdapterList = interfaceNames.ToArray();
            AdapterCount = AdapterList.Length;
        }

        private void SelectedAdapterId_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedAdapter != null)
                Clipboard.SetText(SelectedAdapter.IdHandle);
        }

        private void SelectedAdapterTest_OnClick(object sender, RoutedEventArgs e)
        {
            if (Netst.Settings.Volatile.SelectedTestAdapter != null && Netst.Settings.Volatile.TestRunning)
            {
                MessageBox.Show("There are tests already running for another adapter. Stop them and try again.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Netst.Settings.Volatile.SelectedTestAdapter = SelectedAdapter;
            Netst.Settings.Volatile.MainWindow.NavigateToTab("Network");
        }

        private void RefreshAdapterList_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }
    }
}
