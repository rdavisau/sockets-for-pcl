using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = Sockets.Plugin.Abstractions.SocketException;
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
        /// <param name="multicastOn">The <code>CommsInterface</code> to multicast on. If unspecified, all interfaces will be bound.</param>
        /// <returns></returns>
        public async Task JoinMulticastGroupAsync(string multicastAddress, int port, ICommsInterface multicastOn = null)
        {
            if (multicastOn != null && !multicastOn.IsUsable)
                throw new InvalidOperationException("Cannot multicast on an unusable interface. Check the IsUsable property before attemping to connect.");

            var bindingIp = multicastOn != null ? ((CommsInterface)multicastOn).NativeIpAddress : IPAddress.Any;
            var bindingEp = new IPEndPoint(bindingIp, port);

            var multicastIp = IPAddress.Parse(multicastAddress);

            try
            {
                _backingUdpClient = new UdpClient(bindingEp)
                {
                    EnableBroadcast = true
                };
                ProtectAgainstICMPUnreachable(_backingUdpClient);
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            _messageCanceller = new CancellationTokenSource();
            
            await Task
                .Run(() => this._backingUdpClient.JoinMulticastGroup(multicastIp, this.TTL))
                .WrapNativeSocketExceptions();

            _multicastAddress = multicastAddress;
            _multicastPort = port;

            RunMessageReceiver(_messageCanceller.Token);
        }

        /// <summary>
        ///     Removes the <code>UdpSocketMulticastClient</code> from a joined multicast group.
        /// </summary>
        public Task DisconnectAsync()
        {
            return Task.Run(() =>
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
        public Task SendMulticastAsync(byte[] data)
        {
            return SendMulticastAsync(data, data.Length);
        }

        /// <summary>
        ///     Sends the specified data to the multicast group, previously set using <code>JoinMulticastGroupAsync</code>.
        ///     If a group has not been set, calls will have no effect.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="length">The number of bytes from <c>data</c> to send.</param>
        public Task SendMulticastAsync(byte[] data, int length)
        {
            if (_multicastAddress == null)
                throw new InvalidOperationException("Must join a multicast group before sending.");

            return base.SendToAsync(data, length, _multicastAddress, _multicastPort);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (_messageCanceller != null && !_messageCanceller.IsCancellationRequested)
                _messageCanceller.Cancel();

            base.Dispose();
        }
    }
}