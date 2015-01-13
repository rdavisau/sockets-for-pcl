using System.Net;

namespace Sockets.Plugin
{
    public class NetInterfaceInfo
    {
        public IPAddress Netmask;
        public IPAddress Address;
        public byte[] MacAddress;
        public ushort Index;
        public byte Type;
    }
}