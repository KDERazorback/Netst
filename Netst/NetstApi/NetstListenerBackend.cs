using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Netst.NetstApi
{
    public abstract class NetstListenerBackend : NetstClientBackend
    {
        // IVars

        // Properties
        public List<Client> Clients { get; protected set; } = new List<Client>();
        public int ClientCount => Clients.Count;

        // Methods
    }
}
