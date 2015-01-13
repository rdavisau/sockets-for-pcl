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
    public partial class CommsInterface : ICommsInterface
    {
        /// <summary>
        /// The interface identifier provided by the underlying platform.
        /// </summary>
        public string NativeInterfaceId { get; private set; }

        /// <summary>
        /// The interface name, as provided by the underlying platform.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The IPv4 Address of the interface, if connected. 
        /// </summary>
        public string IpAddress { get; private set; }

        /// <summary>
        /// The IPv4 address of the gateway, if available.
        /// </summary>
        public string GatewayAddress { get; private set; }

        /// <summary>
        /// The IPv4 broadcast address for the interface, if available.
        /// </summary>
        public string BroadcastAddress { get; private set; }

        /// <summary>
        /// The connection status of the interface, if available
        /// </summary>
        public NetworkInterfaceStatus ConnectionStatus { get; private set; }

        /// <summary>
        /// Indicates whether the interface has a network address and can be used for 
        /// sending/receiving data.
        /// </summary>
        public bool IsUsable
        {
            get { return !String.IsNullOrWhiteSpace(IpAddress); }
        }

        private readonly string[] _loopbackAddresses = { "127.0.0.1", "localhost" };

        /// <summary>
        /// Indicates whether the interface is the loopback interface
        /// </summary>
        public bool IsLoopback
        {
            // yes, crude.
            get { return _loopbackAddresses.Contains(IpAddress); }
        }

        protected internal System.Net.NetworkInformation.NetworkInterface NativeInterface;
        
        internal static CommsInterface FromNativeInterface(System.Net.NetworkInformation.NetworkInterface nativeInterface)
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

            var netmask = GetSubnetMask(ip); // implemented natively for each .NET platform

            var broadcast = (ip != null && netmask != null) ? ip.Address.GetBroadcastAddress(netmask).ToString() : null;

            return new CommsInterface
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
        public static async Task<List<CommsInterface>> GetAllNetworkInterfacesAsync()
        {
            var interfaces = await Task.Run(() =>
                System.Net.NetworkInformation.NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Select(FromNativeInterface)
                    .ToList());

            return interfaces;
        }
    }
}
