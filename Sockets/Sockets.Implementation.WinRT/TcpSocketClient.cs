using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Sockets.Plugin.Abstractions;
using System.Threading;

// ReSharper disable once CheckNamespace

namespace Sockets.Plugin
{
    /// <summary>
    ///     Sends and receives data over a TCP socket. Establish a connection with a listening TCP socket using
    ///     <code>ConnectAsync</code>.
    ///     Use the <code>WriteStream</code> and <code>ReadStream</code> properties for sending and receiving data
    ///     respectively.
    /// </summary>
    public class TcpSocketClient : ITcpSocketClient
    {
#if WP80
        private SocketProtectionLevel _secureSocketProtectionLevel = SocketProtectionLevel.Ssl;
#else
        private SocketProtectionLevel _secureSocketProtectionLevel = SocketProtectionLevel.Tls10;
#endif               
        private readonly StreamSocket _backingStreamSocket;
        private readonly int _bufferSize;

        /// <summary>
        ///     Default constructor for <code>TcpSocketClient</code>.
        /// </summary>
        public TcpSocketClient()
        {
            _backingStreamSocket = new StreamSocket();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocketClient"/> class.
        /// </summary>
        /// <param name="bufferSize">Size of the buffer for the write stream.</param>
        public TcpSocketClient(int bufferSize) : this()
        {
            _bufferSize = bufferSize;
        }

        internal TcpSocketClient(StreamSocket nativeSocket, int bufferSize)
        {
            _backingStreamSocket = nativeSocket;
            _bufferSize = bufferSize;
        }

        /// <summary>
        ///     Establishes a TCP connection with the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="address">The address of the endpoint to connect to.</param>
        /// <param name="port">The port of the endpoint to connect to.</param>
        /// <param name="secure">True to enable TLS on the socket.</param>
        /// <param name="timeout">Client specified timout.</param>
        public Task ConnectAsync(string address, int port, bool secure = false, int timeout = 0)
        {
            var hn = new HostName(address);
            var sn = port.ToString();
            var spl = secure ? _secureSocketProtectionLevel : SocketProtectionLevel.PlainSocket;

            // create connection timeout token
            // See https://msdn.microsoft.com/en-us/library/windows/apps/xaml/jj710176.aspx.

            var token = timeout > 0
                            ? new CancellationTokenSource(timeout).Token
                            : CancellationToken.None;

            return  _backingStreamSocket.ConnectAsync(hn, sn, spl).AsTask(CancellationToken.None);
        }

        /// <summary>
        ///     Establishes a TCP connection with the service at the specified address/port pair.
        /// </summary>
        /// <param name="address">The address of the endpoint to connect to.</param>
        /// <param name="service">The service to connect to.</param>
        /// <param name="timeout">Connection timout in msec.</param>
        /// <param name="secure">True to enable TLS on the socket.</param>
        /// <param name="timeout">Client specified timout.</param>
        public Task ConnectAsync(string address, string service, bool secure = false, int timeout = 0)
        {
            var hn = new HostName(address);
            var spl = secure ? _secureSocketProtectionLevel : SocketProtectionLevel.PlainSocket;

            // create connection timeout token
            // See https://msdn.microsoft.com/en-us/library/windows/apps/xaml/jj710176.aspx
            var token = timeout > 0
                            ? new CancellationTokenSource(timeout).Token
                            : CancellationToken.None;

            return _backingStreamSocket.ConnectAsync(hn, service, spl).AsTask(token);
        }

        /// <summary>
        ///     Disconnects from an endpoint previously connected to using <code>ConnectAsync</code>.
        ///     Should not be called on a <code>TcpSocketClient</code> that is not already connected.
        /// </summary>
        public Task DisconnectAsync()
        {
            return Task.Run(() => _backingStreamSocket.Dispose());
        }

        /// <summary>
        ///     Returns the underlying backingField.
        /// </summary>
        public object Socket
        {
            get
            {
                return _backingStreamSocket;
            }
        }

        /// <summary>
        ///     A stream that can be used for receiving data from the remote endpoint.
        /// </summary>
        public Stream ReadStream
        {
            get { return _backingStreamSocket.InputStream.AsStreamForRead(_bufferSize); }
        }

        /// <summary>
        ///     A stream that can be used for sending data to the remote endpoint.
        /// </summary>
        public Stream WriteStream
        {
            get { return _backingStreamSocket.OutputStream.AsStreamForWrite(_bufferSize); }
        }

        /// <summary>
        ///     The address of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public string RemoteAddress
        {
            get { return _backingStreamSocket.Information.RemoteAddress.CanonicalName; }
        }

        /// <summary>
        ///     The port of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public int RemotePort
        {
            get { return Int32.Parse(_backingStreamSocket.Information.RemotePort); }
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
        ~TcpSocketClient()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_backingStreamSocket != null)
                    (_backingStreamSocket).Dispose();
            }
        }
    }
}