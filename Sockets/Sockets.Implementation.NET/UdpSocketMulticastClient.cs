using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

// ReSharper disable once CheckNamespace

namespace Sockets.Plugin
{
    /// <summary>
    ///     Sends and recieves data in a UDP multicast group.
    ///     Join a UDP multicast address using <code>JoinMulticastGroupAsync</code>, then send data using
    ///     <code>SendMulticastAsync</code>.
    /// </summary>
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {
        private string _multicastAddress;
        private int _multicastPort;

        private CancellationTokenSource _messageCanceller;
        private int _ttl = 1;

        /// <summary>
        ///     Joins the multicast group at the specified endpoint.
        /// </summary>
        /// <param name="multicastAddress">The address for the multicast group.</param>
        /// <param name="port">The port for the multicast group.</param>
        /// <returns></returns>
        public async Task JoinMulticastGroupAsync(string multicastAddress, int port)
        {
            var ip = IPAddress.Parse(multicastAddress);
            _backingUdpClient = new UdpClient(port);
            _messageCanceller = new CancellationTokenSource();

            await Task.Run(() => _backingUdpClient.JoinMulticastGroup(ip,TTL));

            _multicastAddress = multicastAddress;
            _multicastPort = port;

            RunMessageReceiver(_messageCanceller.Token);
        }

        /// <summary>
        ///     Removes the <code>UdpSocketMulticastClient</code> from a joined multicast group.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await Task.Run(() =>
            {
                _messageCanceller.Cancel();
                _backingUdpClient.Close();

                _multicastAddress = null;
                _multicastPort = 0;
            });
        }

        /// <summary>
        ///     Gets or sets the Time To Live value for the <code>UdpSocketMulticastClient</code>.
        ///     Must be called before joining a multicast group. 
        /// </summary>
        public int TTL
        {
            get { return _ttl; }
            set { _ttl = value; }
        }

        /// <summary>
        ///     Sends the specified data to the multicast group, previously set using <code>JoinMulticastGroupAsync</code>.
        ///     If a group has not been set, calls will have no effect.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        public async Task SendMulticastAsync(byte[] data)
        {
            if (_multicastAddress == null)
                throw new InvalidOperationException("Must join a multicast group before sending.");

            await base.SendToAsync(data, _multicastAddress, _multicastPort);
        }
    }
}