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
    ///     Base class for .NET UDP socket wrapper.
    /// </summary>
    public abstract class UdpSocketBase
    {
        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Native socket field around which UdpSocketBase wraps.
        /// </summary>
        protected UdpClient _backingUdpClient;

        /// <summary>
        ///     Fired when a UDP datagram has been received.
        /// </summary>
        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;

        internal async void RunMessageReceiver(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var didReceive = false;
                var msg = new UdpReceiveResult();

                try
                {
                    // attempt to read next datagram
                    msg = await _backingUdpClient
                        .ReceiveAsync()
                        .WrapNativeSocketExceptions();
                    
                    didReceive = true;
                }
                catch
                {
                    // exception may occur because we stopped listening
                    // (i.e. cancelled the token) - if so exit loop
                    // otherwise throw.
                    if (!cancellationToken.IsCancellationRequested)
                        throw;
                }

                if (!didReceive)
                    return; // cancelled, exit loop;

                // generate the message received event
                var remoteAddress = msg.RemoteEndPoint.Address.ToString();
                var remotePort = msg.RemoteEndPoint.Port.ToString();
                var data = msg.Buffer;

                var wrapperArgs = new UdpSocketMessageReceivedEventArgs(remoteAddress, remotePort, data);

                // fire
                if (MessageReceived != null)
                    MessageReceived(this, wrapperArgs);
            }
        }

        /// <summary>
        ///     Sends the specified data to the 'default' target of the underlying DatagramSocket.
        ///     There may be no 'default' target. depending on the state of the object.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        protected Task SendAsync(byte[] data)
        {
            return _backingUdpClient
                .SendAsync(data, data.Length)
                .WrapNativeSocketExceptions();
        }

        /// <summary>
        ///     Sends the specified data to the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="address">The remote address to which the data should be sent.</param>
        /// <param name="port">The remote port to which the data should be sent.</param>
        protected Task SendToAsync(byte[] data, string address, int port)
        {
            return _backingUdpClient
                .SendAsync(data, data.Length, address, port)
                .WrapNativeSocketExceptions();
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~UdpSocketBase()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_backingUdpClient != null)
                    ((IDisposable)_backingUdpClient).Dispose();
            }
        }
        
    }
}