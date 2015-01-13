using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Java.Net;
using NetworkInterface = Java.Net.NetworkInterface;

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
            // short circuit on null ip. 
            if (ip == null)
                return null;


            // TODO: Use native java network library rather than incomplete mono/.NET implementation. 
            //       Move this into CommsInterface rather than iterating all adapters for each GetSubnetMaskCall. 
            var interfaces = NetworkInterface.NetworkInterfaces.GetEnumerable<NetworkInterface>().ToList();
            var interfacesWithIPv4Addresses = interfaces
                                                .Where(ni => ni.InterfaceAddresses != null)    
                                                .SelectMany(ni=> ni.InterfaceAddresses
                                                                        .Where(a=> a.Address != null && a.Address.HostAddress != null)
                                                                        .Select(a=> new { NativeInterface = ni, Address = a }))                       
                                                .ToList();

            var ipAddress = ip.Address.ToString();
            var match = interfacesWithIPv4Addresses.FirstOrDefault(ni => ni.Address.Address.HostAddress == ipAddress);

            if (match == null)
                return null;
            
            var networkPrefixLength = match.Address.NetworkPrefixLength;
            var netMask = AndroidNetworkExtensions.GetSubnetAddress(ipAddress, networkPrefixLength);
            
            return IPAddress.Parse(netMask);
        }
    }

    // thank you deapsquatter - https://gist.github.com/deapsquatter/5644550
    public static class AndroidNetworkExtensions
    {
        public static IEnumerable<T> GetEnumerable<T>(this Java.Util.IEnumeration enumeration) where T : class
        {
            while (enumeration.HasMoreElements)
                yield return enumeration.NextElement() as T;
        }

        /// <summary>
        /// Calculates the subnet address for an ip address given its prefix link.
        /// Returns the ip as a string in dotted quad notation.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="prefixLength"></param>
        /// <returns></returns>
        public static string GetSubnetAddress(string ipAddress, int prefixLength)
        {
            var maskBits = Enumerable.Range(0, 32)
                .Select(i => i < prefixLength)
                .ToArray();

            return maskBits
                .ToBytes()
                .ToDottedQuadNotation();
        }

        /// <summary>
        /// Converts an array of bools to an array of bytes, 8 bits per byte.
        /// Expects most significant bit first. 
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this bool[] bits)
        {
            var theseBits = bits.Reverse().ToArray();
            var ba = new BitArray(theseBits);

            var bytes = new byte[theseBits.Length / 8];
            ((ICollection)ba).CopyTo(bytes, 0);

            bytes = bytes.Reverse().ToArray();

            return bytes;
        }

        /// <summary>
        /// Converts dotted quad representation of an ip address into a byte array. 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static byte[] GetAddressBytes(string ipAddress)
        {
            var ipBytes = new byte[4];

            var parsesResults =
                ipAddress.Split('.')
                    .Select((p, i) => byte.TryParse(p, out ipBytes[i]))
                    .ToList();


            var valid = (parsesResults.Count == 4 && parsesResults.All(r => r));

            if (valid)
                return ipBytes;
            else
                throw new InvalidOperationException("The string provided did not appear to be a valid IP Address");
        }

        /// <summary>
        /// Converts a byte array into dotted quad representation.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToDottedQuadNotation(this byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
                throw new InvalidOperationException("Byte array has an invalid byte count for dotted quad conversion");

            return String.Join(".", bytes.Select(b => ((int)b).ToString()));
        }
    }
}
