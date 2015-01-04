using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Sockets.Plugin.Abstractions;

// ReSharper disable once CheckNamespace

namespace Sockets.Plugin
{
    /// <summary>
    ///     Binds to a port and listens for TCP connections.
    ///     Use <code>StartListeningAsync</code> to bind to a local port, then handle <code>ConnectionReceived</code> events as
    ///     clients connect.
    /// </summary>
    public class TcpSocketListener : ITcpSocketListener
    {
        private StreamSocketListener _backingStreamSocketListener;
        private CancellationTokenSource _listenCanceller;

        /// <summary>
        ///     Fired when a new TCP connection has been received.
        ///     Use the <code>SocketClient</code> property of the <code>TcpSocketListenerConnectEventArgs</code>
        ///     to get a <code>TcpSocketClient</code> representing the connection for sending and receiving data.
        /// </summary>
        public EventHandler<TcpSocketListenerConnectEventArgs> ConnectionReceived { get; set; }

        /// <summary>
        ///     Binds the <code>TcpSocketListener</code> to the specified port on all endpoints and listens for TCP connections.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <returns></returns>
        public async Task StartListeningAsync(int port)
        {
            _listenCanceller = new CancellationTokenSource();
            _backingStreamSocketListener = new StreamSocketListener();

            _backingStreamSocketListener.ConnectionReceived += (sender, args) =>
            {
                var nativeSocket = args.Socket;
                var wrappedSocket = new TcpSocketClient(nativeSocket);

                var eventArgs = new TcpSocketListenerConnectEventArgs(wrappedSocket);
                if (ConnectionReceived != null)
                    ConnectionReceived(this, eventArgs);
            };

            await _backingStreamSocketListener.BindServiceNameAsync(port.ToString());
        }

        /// <summary>
        ///     Stops the <code>TcpSocketListener</code> from listening for new tcp connections.
        ///     This does not disconnect existing connections.
        /// </summary>
        public async Task StopListeningAsync()
        {
            await Task.Run(() =>
            {
                _listenCanceller.Cancel();
                _backingStreamSocketListener.Dispose();
                _backingStreamSocketListener = null;
            });
        }

        /// <summary>
        ///     The port to which the TcpSocketListener is currently bound
        /// </summary>
        public int LocalPort
        {
            get
            {
                return _backingStreamSocketListener != null
                    ? Int32.Parse(_backingStreamSocketListener.Information.LocalPort)
                    : 0;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~TcpSocketListener()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_backingStreamSocketListener != null)
                    (_backingStreamSocketListener).Dispose();
            }
        }
    }
}