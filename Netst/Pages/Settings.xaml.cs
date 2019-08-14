using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Netst.NetstApi;
using Netst.NetstApi.Broadcast;

namespace Netst.Pages
{
    /// <summary>
    /// Lógica de interacción para Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public static readonly DependencyProperty PreferUdpProperty = DependencyProperty.Register(
            "PreferUdp", typeof(bool), typeof(Settings), new PropertyMetadata(false));

        public bool PreferUdp
        {
            get { return (bool) GetValue(PreferUdpProperty); }
            set { SetValue(PreferUdpProperty, value); }
        }

        public static readonly DependencyProperty UseTimerBasedHandlingProperty = DependencyProperty.Register(
            "UseTimerBasedHandling", typeof(bool), typeof(Settings), new PropertyMetadata(false));

        public bool UseTimerBasedHandling
        {
            get { return (bool) GetValue(UseTimerBasedHandlingProperty); }
            set { SetValue(UseTimerBasedHandlingProperty, value); }
        }

        public static readonly DependencyProperty AvailableBufferSizesProperty = DependencyProperty.Register(
            "AvailableBufferSizes", typeof(int[]), typeof(Settings), new PropertyMetadata(null));

        public int[] AvailableBufferSizes
        {
            get { return (int[]) GetValue(AvailableBufferSizesProperty); }
            set { SetValue(AvailableBufferSizesProperty, value); }
        }

        public static readonly DependencyProperty SelectedTxBufferSizeProperty = DependencyProperty.Register(
            "SelectedTxBufferSize", typeof(int), typeof(Settings), new PropertyMetadata(null));

        public int SelectedTxBufferSize
        {
            get { return (int) GetValue(SelectedTxBufferSizeProperty); }
            set { SetValue(SelectedTxBufferSizeProperty, value); }
        }

        public static readonly DependencyProperty SelectedRxBufferSizeProperty = DependencyProperty.Register(
            "SelectedRxBufferSize", typeof(int), typeof(Settings), new PropertyMetadata(null));

        public int SelectedRxBufferSize
        {
            get { return (int) GetValue(SelectedRxBufferSizeProperty); }
            set { SetValue(SelectedRxBufferSizeProperty, value); }
        }

        public static readonly DependencyProperty ThreadCpuPinningProperty = DependencyProperty.Register(
            "ThreadCpuPinning", typeof(bool), typeof(Settings), new PropertyMetadata(false));

        public bool ThreadCpuPinning
        {
            get { return (bool) GetValue(ThreadCpuPinningProperty); }
            set { SetValue(ThreadCpuPinningProperty, value); }
        }

        public static readonly DependencyProperty AnnounceServerProperty = DependencyProperty.Register(
            "AnnounceServer", typeof(bool), typeof(Settings), new PropertyMetadata(true));

        public bool AnnounceServer
        {
            get { return (bool) GetValue(AnnounceServerProperty); }
            set { SetValue(AnnounceServerProperty, value); }
        }

        public static readonly DependencyProperty TrackServersProperty = DependencyProperty.Register(
            "TrackServers", typeof(bool), typeof(Settings), new PropertyMetadata(true));

        public bool TrackServers
        {
            get { return (bool) GetValue(TrackServersProperty); }
            set { SetValue(TrackServersProperty, value); }
        }

        public static readonly DependencyProperty ShowOwnAnnouncerProperty = DependencyProperty.Register(
            "ShowOwnAnnouncer", typeof(bool), typeof(Settings), new PropertyMetadata(false));

        public bool ShowOwnAnnouncer
        {
            get { return (bool) GetValue(ShowOwnAnnouncerProperty); }
            set { SetValue(ShowOwnAnnouncerProperty, value); }
        }

        public Settings()
        {
            InitializeComponent();
        }

        private void ApplyChanges_OnClick(object sender, RoutedEventArgs e)
        {
            Netst.Settings.Persistent.PreferUdp = PreferUdp;
            Netst.Settings.Persistent.UseTxRxTimers = UseTimerBasedHandling;

            Netst.Settings.Persistent.AvailableBufferSizes = AvailableBufferSizes;
            Netst.Settings.Persistent.TxBufferSize = SelectedTxBufferSize;
            Netst.Settings.Persistent.RxBufferSize = SelectedRxBufferSize;

            Netst.Settings.Persistent.ThreadCpuPinning = ThreadCpuPinning;

            Netst.Settings.Volatile.PreferUdp = PreferUdp;
            NetstClientBackend.UseTimers = UseTimerBasedHandling;

            Netst.Settings.Persistent.AnnounceServer = AnnounceServer;
            Netst.Settings.Persistent.TrackServers = TrackServers;
            Netst.Settings.Persistent.HideOwnAnnouncerEntries = !ShowOwnAnnouncer;

            if (AnnounceServer && !Netst.Settings.Volatile.IsAnnouncing &&
                Netst.Settings.Volatile.ActiveServer != null && Netst.Settings.Volatile.ActiveServer.Started)
                Netst.Settings.Volatile.TryAttachAnnouncer();

            if (TrackServers && !Netst.Settings.Volatile.IsTracking)
                Netst.Settings.Volatile.TryAttachTracker();

            MessageBox.Show("Settings applied successfully!", "Settings saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        

        private void Settings_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool && (bool)e.NewValue)
                UpdateView();
        }

        public void UpdateView()
        {
            PreferUdp = Netst.Settings.Persistent.PreferUdp;
            UseTimerBasedHandling = Netst.Settings.Persistent.UseTxRxTimers;

            AvailableBufferSizes = Netst.Settings.Persistent.AvailableBufferSizes;
            SelectedTxBufferSize = Netst.Settings.Persistent.TxBufferSize;
            SelectedRxBufferSize = Netst.Settings.Persistent.RxBufferSize;

            ThreadCpuPinning = Netst.Settings.Persistent.ThreadCpuPinning;

            AnnounceServer = Netst.Settings.Persistent.AnnounceServer;
            TrackServers = Netst.Settings.Persistent.TrackServers;
            ShowOwnAnnouncer = !Netst.Settings.Persistent.HideOwnAnnouncerEntries;
        }

        private void DefaultChanges_OnClick(object sender, RoutedEventArgs e)
        {
            Netst.Settings.Persistent = new Persistent();
            UpdateView();
        }

        private void RevertChanges_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }

        private void Ionicons_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://ionicons.com/");
        }

        private void LoadingIo_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://loading.io/");
        }
    }
}
