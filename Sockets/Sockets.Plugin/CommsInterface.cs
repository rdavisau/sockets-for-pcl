using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace Sockets.Plugin
{
    /// <summary>
    /// Provides a summary of an available network interface on the device.
    /// </summary>
    public class CommsInterface : ICommsInterface
    {
        /// <summary>
        /// The interface identifier provided by the underlying platform.
        /// </summary>
        public string NativeInterfaceId
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
            set { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        /// The interface name, as provided by the underlying platform.
        /// </summary>
        public string Name
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        /// The IPv4 Address of the interface, if connected. 
        /// </summary>
        public string IpAddress
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        /// The IPv4 address of the gateway, if available.
        /// </summary>
        public string GatewayAddress
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        /// The IPv4 broadcast address for the interface, if available.
        /// </summary>
        public string BroadcastAddress
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        /// The connection status of the interface, if available
        /// </summary>
        public NetworkInterfaceStatus ConnectionStatus
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        public bool IsUsable
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        public bool IsLoopback
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        // TODO: Move to singleton, rather than static method?
        /// <summary>
        /// Retrieves information on the IPv4 network interfaces available.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CommsInterface>> GetAllNetworkInterfacesAsync()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

    }
}
