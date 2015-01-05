using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace Sockets.Plugin
{
    class NetworkInterfaceSummary : INetworkInterfaceSummary
    {
        public string NativeInterfaceId
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
            set { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        public string Name
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        public string IpAddress
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        public string GatewayAddress
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        public string BroadcastAddress
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        public NetworkInterfaceStatus ConnectionStatus
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }
    }
}
