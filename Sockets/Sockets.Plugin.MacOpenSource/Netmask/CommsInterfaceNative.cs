using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace Sockets.Plugin
{
    public partial class CommsInterface
    {
        /// <summary>
        /// UnicastIPAddressInformation.IPv4Mask is not implemented in Xamarin. This method sits in a partial class definition
        /// on each native platform and retrieves the netmask in whatever way it can be done for each platform. 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        protected static IPAddress GetSubnetMask(UnicastIPAddressInformation ip)
        {
            var nativeInterfaceInfo = NetInfo.GetInterfaceInfo();
            var match = nativeInterfaceInfo.FirstOrDefault(ni => ni != null && ni.Address != null && ip != null && ip.Address != null && Equals(ni.Address, ip.Address));

            return match != null ? match.Netmask : null;
        }
    }

}
