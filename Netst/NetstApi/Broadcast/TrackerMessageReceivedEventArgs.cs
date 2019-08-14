using System;
using System.Net;

namespace Netst.NetstApi.Broadcast
{
    public class TrackerMessageReceivedEventArgs
    {
        public IPEndPoint RemoteEndPoint { get; protected set; }
        public byte[] ReceivedData { get; protected set; }
        public Exception Exception { get; protected set; }
        public NodeInfo RemoteNodeInfo { get; protected set; }
        public bool Failed => Exception != null;

        public TrackerMessageReceivedEventArgs(IPEndPoint source, byte[] data, Exception exception)
        {
            RemoteEndPoint = source;
            ReceivedData = data;
            Exception = exception;
        }

        public TrackerMessageReceivedEventArgs(IPEndPoint source, byte[] data, NodeInfo remoteInfo)
        {
            RemoteEndPoint = source;
            ReceivedData = data;
            RemoteNodeInfo = remoteInfo;
        }
    }
}
