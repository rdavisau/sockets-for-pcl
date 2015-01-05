using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace Sockets.Implementation.NET
{
    public class NetworkInterfaceSummary : INetworkInterfaceSummary
    {
        public string NativeInterfaceId { get; private set; }
        
        public string Name { get; private set; }

        public string IpAddress { get; private set; }

        public int NetmaskLength { get; private set; }
        
        public string GatewayAddress { get; private set; }

        public string BroadcastAddress { get; private set; }

        public NetworkInterfaceStatus ConnectionStatus { get; private set; }

        public static NetworkInterfaceSummary FromNativeInterface(NetworkInterface nativeInterface)
        {           
            var ip = 
                nativeInterface
                    .GetIPProperties()
                    .UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);

            var gateway =
                nativeInterface
                    .GetIPProperties()
                    .GatewayAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .FirstOrDefault();

            var broadcast = ip != null ? ip.Address.GetBroadcastAddress(ip.IPv4Mask).ToString() : null;

            return new NetworkInterfaceSummary
            {
                NativeInterfaceId = nativeInterface.Id,
                Name = nativeInterface.Name,
                IpAddress = ip != null ? ip.Address.ToString() : null,
                GatewayAddress = gateway,
                BroadcastAddress = broadcast,
                ConnectionStatus = nativeInterface.OperationalStatus.ToNetworkInterfaceStatus()
            };
        }
    }
}
