namespace Sockets.Plugin.Abstractions
{
    /// <summary>
    ///     Fires when a udp listener or udp multicast client receives a udp datagram.
    /// </summary>
    public class UdpSocketMessageReceivedEventArgs
    {
        private readonly string _remotePort;
        private readonly string _remoteAddress;
        private readonly byte[] _byteData;

        /// <summary>
        ///     Constructor for the <code>UdpSocketMessageReceivedEventArgs.</code>
        /// </summary>
        /// <param name="remoteAddress">Remote address of the received datagram.</param>
        /// <param name="remotePort">Remote port of the received datagram.</param>
        /// <param name="byteData">Datagram contents.</param>
        public UdpSocketMessageReceivedEventArgs(string remoteAddress, string remotePort, byte[] byteData)
        {
            _remoteAddress = remoteAddress;
            _remotePort = remotePort;
            _byteData = byteData;
        }

        /// <summary>
        ///     Remote address of the received datagram.
        /// </summary>
        public string RemoteAddress
        {
            get { return _remoteAddress; }
        }

        /// <summary>
        ///     Remote port of the received datagram.
        /// </summary>
        public string RemotePort
        {
            get { return _remotePort; }
        }

        /// <summary>
        ///     Datagram contents.
        /// </summary>
        public byte[] ByteData
        {
            get { return _byteData; }
        }
    }
}