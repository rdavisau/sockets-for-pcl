using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace Sockets.Plugin
{
    /// <summary>
    /// Provides a summary of an available network interface on the device.
    /// </summary>
    public class NetworkInterfaceSummary : INetworkInterfaceSummary
    {
        public string NativeInterfaceId { get; private set; }
        
        public string Name { get; private set; }

        public string IpAddress { get; private set; }
      
        public string GatewayAddress { get; private set; }

        public string BroadcastAddress { get; private set; }

        public NetworkInterfaceStatus ConnectionStatus { get; private set; }

        protected internal NetworkInterface NativeInterface;
        
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
                ConnectionStatus = nativeInterface.OperationalStatus.ToNetworkInterfaceStatus(),
                NativeInterface = nativeInterface
            };
        }

        // TODO: Move to singleton, rather than static method?
        /// <summary>
        /// Retrieves information on the IPv4 network interfaces available.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<NetworkInterfaceSummary>> GetAllNetworkInterfaceSummariesAsync()
        {
            var interfaces = await Task.Run(() =>
                NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Select(FromNativeInterface)
                    .ToList());

            return interfaces;
        }
    }
}
