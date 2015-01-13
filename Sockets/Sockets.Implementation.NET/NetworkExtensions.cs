using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Sockets.Plugin.Abstractions;

namespace Sockets.Plugin
{
    public static class NetworkExtensions
    {
        public static CommsInterface ToNetworkInterfaceSummary(this System.Net.NetworkInformation.NetworkInterface nativeInterface)
        {
            return CommsInterface.FromNativeInterface(nativeInterface);
        }

        public static NetworkInterfaceStatus ToNetworkInterfaceStatus(this OperationalStatus nativeStatus)
        {
            switch (nativeStatus)
            {
                case OperationalStatus.Up:
                    return NetworkInterfaceStatus.Connected;
                case OperationalStatus.Down:
                    return NetworkInterfaceStatus.Disconnected;
                case OperationalStatus.Unknown:
                    return NetworkInterfaceStatus.Unknown;

                /*
                 * Treat remaining enumerations as "Unknown".
                 * It's unlikely that they will be returned on 
                 * a mobile device anyway. 
                 * 
                    case OperationalStatus.Testing:
                        break;
                    case OperationalStatus.Unknown:
                        break;
                    case OperationalStatus.Dormant:
                        break;
                    case OperationalStatus.NotPresent:
                        break;
                    case OperationalStatus.LowerLayerDown:
                        break;
                 */

                default:
                    return NetworkInterfaceStatus.Unknown;
            }
            
        }

        /// <summary>
        /// Determines the broadcast address for a given IPAddress
        /// Adapted from http://blogs.msdn.com/b/knom/archive/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks.aspx
        /// </summary>
        /// <param name="address"></param>
        /// <param name="subnetMask"></param>
        /// <returns></returns>
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            var addressBytes = address.GetAddressBytes();
            var subnetBytes = subnetMask.GetAddressBytes();

            var broadcastBytes = addressBytes.Zip(subnetBytes, (a, s) => (byte) (a | (s ^ 255))).ToArray();

            return new IPAddress(broadcastBytes);
        }
    }
}