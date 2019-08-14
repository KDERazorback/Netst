using com.razorsoftware.SettingsLib;

namespace Netst
{
    public class Persistent : Document
    {
        public bool PreferUdp = false;
        public bool UseTxRxTimers = false;

        public int[] AvailableBufferSizes = new int[]
        {
            512,
            1024,
            2048,
            4096,
            8192,
            12288,
            16384,
            24576,
            32768,
        };

        public int TxBufferSize = 4096;
        public int RxBufferSize = 4096;

        public bool ThreadCpuPinning = true;

        public bool AnnounceServer = true;
        public bool TrackServers = true;
        public bool HideOwnAnnouncerEntries = true;
    }
}
