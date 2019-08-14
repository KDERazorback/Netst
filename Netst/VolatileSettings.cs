using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Netst.NetstApi;
using Netst.NetstApi.Broadcast;
using Netst.ValueConverters;

namespace Netst
{
    public class VolatileSettings
    {
        public MainWindow MainWindow { get; set; }
        public Dispatcher PrimaryDispatcher { get; set; }
        public Uri MainWindowFrame { get; set; }
        public NetstNetworkAdapter SelectedTestAdapter { get; set; }
        public bool TestRunning { get; set; }
        public bool PreferUdp { get; set; }
        public Server ActiveServer { get; set; }
        public Announcer ActiveAnnouncer { get; protected set; }
        public Tracker ActiveTracker { get; protected set; }

        public bool IsAnnouncing => ActiveAnnouncer != null && ActiveAnnouncer.Started;
        public bool IsTracking => ActiveTracker != null && ActiveAnnouncer.Started;

        public delegate void ClientDiscoveredDelegate(Tracker tracker, IPEndPoint endpoint, NodeInfo info);

        public event ClientDiscoveredDelegate ClientDiscovered;

        public void TryAttachTracker()
        {
            if (IsTracking)
                return;

            if (!Settings.Persistent.TrackServers)
                return;

            ActiveTracker = new Tracker();
            ActiveTracker.MessageReceived += ActiveTrackerOnMessageReceived;
            ActiveTracker.Start();
        }

        private void ActiveTrackerOnMessageReceived(Tracker tracker, TrackerMessageReceivedEventArgs trackerMessageReceivedEventArgs)
        {
            if (trackerMessageReceivedEventArgs.Failed)
                return;

            if (string.Equals(trackerMessageReceivedEventArgs.RemoteNodeInfo.AddressStr,
                    ActiveServer.Address.ToString(), StringComparison.OrdinalIgnoreCase) &&
                Netst.Settings.Persistent.HideOwnAnnouncerEntries)
                return;

            OnClientDiscovered(tracker, trackerMessageReceivedEventArgs.RemoteEndPoint, trackerMessageReceivedEventArgs.RemoteNodeInfo);
        }

        public void TryAttachAnnouncer()
        {
            if (!Settings.Persistent.AnnounceServer)
                return;

            ActiveAnnouncer = new Announcer(ActiveServer);
            ActiveAnnouncer.MachineDataPrepared += ActiveAnnouncerOnMachineDataPrepared;
            ActiveAnnouncer.Start();
        }

        private void ActiveAnnouncerOnMachineDataPrepared(Announcer announcer, Server server, ref NodeInfo info)
        {
            List<string[]> data = new List<string[]>(info.ExtraParams);
            data.Add(new string[]
            {
                "adapter-type", SelectedTestAdapter != null
                    ? new NetworkAdapterTypeToStringConverter()
                        .Convert(SelectedTestAdapter, typeof(string), null, CultureInfo.InvariantCulture)?.ToString()
                    : ""
            });
            data.Add(new string[] {"adapter-speed", SelectedTestAdapter?.SpeedString});
            data.Add(new string[] {"adapter-name", SelectedTestAdapter?.Name});
            data.Add(new string[] {"adapter-mac", FormatMacAddress(SelectedTestAdapter?.MacAddress)});

            info.ExtraParams = data.ToArray();
        }

        protected virtual void OnClientDiscovered(Tracker tracker, IPEndPoint endpoint, NodeInfo info)
        {
            ClientDiscovered?.Invoke(tracker, endpoint, info);
        }

        public string FormatMacAddress(string mac)
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < mac.Length; i++)
            {
                if (i % 2 == 0 && i > 0)
                    str.Append(":");

                str.Append(mac[i]);
            }

            return str.ToString();
        }
    }
}
