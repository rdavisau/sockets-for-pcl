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
    ///     Binds to a port and listens for TCP connections.
    ///     Use <code>StartListeningAsync</code> to bind to a local port, then handle <code>ConnectionReceived</code> events as
    ///     clients connect.
    /// </summary>
    public class TcpSocketListener : ITcpSocketListener
    {
        private TcpListener _backingTcpListener;
        private CancellationTokenSource _listenCanceller;

        /// <summary>
        ///     Fired when a new TCP connection has been received.
        ///     Use the <code>SocketClient</code> property of the <code>TcpSocketListnerConnectEventArgs</code>
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
            await Task.Run(() =>
            {
                _listenCanceller = new CancellationTokenSource();

                _backingTcpListener = new TcpListener(IPAddress.Any, port);
                _backingTcpListener.Start();

                WaitForConnections(_listenCanceller.Token);
            });
        }

        /// <summary>
        ///     Stops the <code>TcpSocketListener</code> from listening for new TCP connections.
        ///     This does not disconnect existing connections.
        /// </summary>
        public async Task StopListeningAsync()
        {
            await Task.Run(
                () =>
                {
                    _listenCanceller.Cancel();
                    _backingTcpListener.Stop();
                    _backingTcpListener = null;
                });
        }

        /// <summary>
        ///     The port to which the TcpSocketListener is currently bound
        /// </summary>
        public int LocalPort
        {
            get { return ((IPEndPoint) (_backingTcpListener.LocalEndpoint)).Port; }
        }

#pragma warning disable 4014
        private void WaitForConnections(CancellationToken cancelToken)
        {
            Task.Factory.StartNew(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var nativeClient = await Task.Run(() => _backingTcpListener.AcceptTcpClient(), cancelToken);
                    var wrappedClient = new TcpSocketClient(nativeClient);

                    var eventArgs = new TcpSocketListenerConnectEventArgs(wrappedClient);
                    if (ConnectionReceived != null)
                        ConnectionReceived(this, eventArgs);
                }
            },
                cancelToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
#pragma warning restore 4014
    }
}