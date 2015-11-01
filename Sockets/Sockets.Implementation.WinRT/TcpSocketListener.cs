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
        private readonly int _bufferSize;

        public TcpSocketListener()
        {
        }

        public TcpSocketListener(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        /// <summary>
        ///     Fired when a new TCP connection has been received.
        ///     Use the <code>SocketClient</code> property of the <code>TcpSocketListenerConnectEventArgs</code>
        ///     to get a <code>TcpSocketClient</code> representing the connection for sending and receiving data.
        /// </summary>
        public event EventHandler<TcpSocketListenerConnectEventArgs> ConnectionReceived;

        /// <summary>
        ///     Binds the <code>TcpSocketListener</code> to the specified port on all endpoints and listens for TCP connections.
        /// </summary>
        /// <param name="port">The port to listen on. If '0', selection is delegated to the operating system.</param>
        /// <param name="listenOn">The <code>CommsInterface</code> to listen on. If unspecified, all interfaces will be bound.</param>
        /// <returns></returns>
        public Task StartListeningAsync(int port, ICommsInterface listenOn = null)
        {
            if (listenOn != null && !listenOn.IsUsable)
                throw new InvalidOperationException("Cannot listen on an unusable interface. Check the IsUsable property before attemping to bind.");
            
            _listenCanceller = new CancellationTokenSource();
            _backingStreamSocketListener = new StreamSocketListener();

            _backingStreamSocketListener.ConnectionReceived += (sender, args) =>
            {
                var nativeSocket = args.Socket;
                var wrappedSocket = new TcpSocketClient(nativeSocket, _bufferSize);

                var eventArgs = new TcpSocketListenerConnectEventArgs(wrappedSocket);
                if (ConnectionReceived != null)
                    ConnectionReceived(this, eventArgs);
            };

            var sn = port == 0 ? "" : port.ToString();
#if !WP80    
            if (listenOn != null)
            {
                var adapter = ((CommsInterface)listenOn).NativeNetworkAdapter;

                return _backingStreamSocketListener
                            .BindServiceNameAsync(sn, SocketProtectionLevel.PlainSocket, adapter)
                            .AsTask();
            }
            else
#endif
                return _backingStreamSocketListener
                            .BindServiceNameAsync(sn)
                            .AsTask();
        }
        
        /// <summary>
        ///     Stops the <code>TcpSocketListener</code> from listening for new tcp connections.
        ///     This does not disconnect existing connections.
        /// </summary>
        public Task StopListeningAsync()
        {   
            return Task.Run(() =>
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