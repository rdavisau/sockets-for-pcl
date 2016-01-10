using System;
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
    ///     Sends UDP data to arbitrary endpoints.
    ///     If data is to be sent to a single endpoint only, use <code>ConnectAsync</code> to specify a default endpoint to
    ///     which data will be sent,
    ///     and send data with <code>SendAsync</code>.
    /// </summary>
    public class UdpSocketClient : UdpSocketBase, IUdpSocketClient
    {
        private CancellationTokenSource _messageCanceller;

        /// <summary>
        ///     Default constructor for <code>UdpSocketClient.</code>
        /// </summary>
        public UdpSocketClient()
        {
            try
            {
                _backingUdpClient = new UdpClient
                {
                    EnableBroadcast = true
                };
                ProtectAgainstICMPUnreachable(_backingUdpClient);
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }
        }

        /// <summary>
        ///     Sets the endpoint at the specified address/port pair as the 'default' target of sent data.
        ///     After calling <code>ConnectAsync</code>, use <code>SendAsync</code> to send data to the default target.
        /// </summary>
        /// <param name="address">The remote address for the default target.</param>
        /// <param name="port">The remote port for the default target.</param>
        public Task ConnectAsync(string address, int port)
        {
            _messageCanceller = new CancellationTokenSource();

            return Task.Run(() => {
                _backingUdpClient.Connect(address, port);
                base.RunMessageReceiver(_messageCanceller.Token);
            })
            .WrapNativeSocketExceptions();
        }

        /// <summary>
        ///     Unsets the 'default' target of sent data.
        ///     After calling <code>DisconnectAsync</code>, calls to <code>SendAsync</code> will have no effect.
        /// </summary>
        public Task DisconnectAsync()
        {
            return Task.Run(() => {
                if (_messageCanceller != null)
                {
                    _messageCanceller.Cancel();
                    _messageCanceller.Dispose();
                    _messageCanceller = null;
                }
                if (_backingUdpClient != null)
                {
                    _backingUdpClient.Close();
                }
            });
        }

        /// <summary>
        ///     Sends the specified data to the 'default' target of the <code>UdpSocketClient</code>, previously set using
        ///     <code>ConnectAsync</code>.
        ///     If the 'default' target has not been set, calls will have no effect.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        public new Task SendAsync(byte[] data)
        {
            return base.SendAsync(data);
        }

        /// <summary>
        ///     Sends the specified data to the 'default' target of the underlying DatagramSocket.
        ///     There may be no 'default' target. depending on the state of the object.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        /// <param name="length">The number of bytes from <c>data</c> to send.</param>
        public new Task SendAsync(byte[] data, int length)
        {
            return base.SendAsync(data, length);
        }

        /// <summary>
        ///     Sends the specified data to the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="address">The remote address to which the data should be sent.</param>
        /// <param name="port">The remote port to which the data should be sent.</param>
        public new Task SendToAsync(byte[] data, string address, int port)
        {
            return base.SendToAsync(data, address, port);
        }

        /// <summary>
        ///     Sends the specified data to the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="length">The number of bytes from <c>data</c> to send.</param>
        /// <param name="address">The remote address to which the data should be sent.</param>
        /// <param name="port">The remote port to which the data should be sent.</param>
        public new Task SendToAsync(byte[] data, int length, string address, int port)
        {
            return base.SendToAsync(data, length, address, port);
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
