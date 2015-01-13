using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace Sockets.Plugin
{
    public class NetInfo
    {

        [DllImport("libc")]
        static extern int getifaddrs(out IntPtr ifap);

        [DllImport("libc")]
        static extern void freeifaddrs(IntPtr ifap);

        const int AF_INET = 2;
        const int AF_INET6 = 30;
        const int AF_LINK = 18;
        // The code in this event handler is essentially a trimmed down version
        // of `MacOsNetworkInterface.ImplGetAllNetworkInterfaces()`, from:
        // `mcs/class/System/System.Net.NetworkInformation/NetworkInterface.cs`

        public static List<NetInterfaceInfo> GetInterfaceInfo()
        {
            var netInterfaceInfos = new Dictionary<string, NetInterfaceInfo>();

            IntPtr ifap;
            if (getifaddrs(out ifap) != 0)
                throw new SystemException("getifaddrs() failed");

            try
            {
                IntPtr next = ifap;
                while (next != IntPtr.Zero)
                {
                    ifaddrs addr = (ifaddrs)Marshal.PtrToStructure(next, typeof(ifaddrs));

                    if (addr.ifa_addr != IntPtr.Zero && (((IFF)addr.ifa_flags & IFF.UP) != 0) &&
                        ((((IFF)addr.ifa_flags) & IFF.LOOPBACK) == 0))
                    {
                        byte sa_family = ((sockaddr)Marshal.PtrToStructure(addr.ifa_addr, typeof(sockaddr))).sa_family;

                        NetInterfaceInfo info;

                        if (!(netInterfaceInfos.TryGetValue(addr.ifa_name, out info)))
                        {
                            info = new NetInterfaceInfo();
                            netInterfaceInfos.Add(addr.ifa_name, info);
                        }

                        if (sa_family == AF_INET)
                        {
                            info.Netmask =
                                new IPAddress(
                                    ((sockaddr_in)Marshal.PtrToStructure(addr.ifa_netmask, typeof(sockaddr_in)))
                                        .sin_addr);
                            info.Address =
                                new IPAddress(
                                    ((sockaddr_in)Marshal.PtrToStructure(addr.ifa_addr, typeof(sockaddr_in))).sin_addr);
                        }
                        else if (sa_family == AF_LINK)
                        {
                            sockaddr_dl sockaddrdl = new sockaddr_dl();
                            sockaddrdl.Read(addr.ifa_addr);

                            info.MacAddress = new byte[(int)sockaddrdl.sdl_alen];
                            Array.Copy(sockaddrdl.sdl_data, sockaddrdl.sdl_nlen, info.MacAddress, 0,
                                Math.Min(info.MacAddress.Length, sockaddrdl.sdl_data.Length - sockaddrdl.sdl_nlen));
                            info.Index = sockaddrdl.sdl_index;
                            info.Type = sockaddrdl.sdl_type;
                        }
                    }
                    next = addr.ifa_next;
                }
            }
            finally
            {
                freeifaddrs(ifap);
            }

            return netInterfaceInfos.Select(ni => ni.Value).ToList();

        }

    }
}