using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography.Certificates;
using Sockets.Plugin.Abstractions;

// ReSharper disable once CheckNamespace

namespace Sockets.Plugin
{
    /// <summary>
    ///     Sends and receives data over a TCP socket. Establish a connection with a listening TCP socket using
    ///     <code>ConnectAsync</code>.
    ///     Use the <code>WriteStream</code> and <code>ReadStream</code> properties for sending and receiving data
    ///     respectively.
    /// </summary>
    public class TcpSocketClient : ITcpSocketClient, IExposeBackingSocket
    {
#if WP80
        private SocketProtectionLevel _secureSocketProtectionLevel = SocketProtectionLevel.Ssl;
#else
        private SocketProtectionLevel _secureSocketProtectionLevel = SocketProtectionLevel.Tls10;
#endif               
        private StreamSocket _backingStreamSocket;
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
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="ignoreSSLErrors">True to ignore SSL errors.</param>
        public Task ConnectAsync(string address, int port, bool secure = false, CancellationToken cancellationToken = default(CancellationToken), bool ignoreSSLErrors = false)
        {
            var service = port.ToString();
            return ConnectAsync(address, service, secure, cancellationToken, ignoreSSLErrors);
        }

        /// <summary>
        ///     Establishes a TCP connection with the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="address">The address of the endpoint to connect to.</param>
        /// <param name="service">The service of the endpoint to connect to.</param>
        /// <param name="secure">True to enable TLS on the socket.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="ignoreSSLErrors">True to ignore SSL errors.</param>
        public Task ConnectAsync(string address, string service, bool secure = false, CancellationToken cancellationToken = default(CancellationToken), bool ignoreSSLErrors = false)
        {
            var hn = new HostName(address);
            var sn = service;
            var spl = secure ? _secureSocketProtectionLevel : SocketProtectionLevel.PlainSocket;

#if !WINDOWS_PHONE
            if (ignoreSSLErrors)
            {
                //List is based on this forum post https://social.msdn.microsoft.com/Forums/en-US/838ba310-9e54-4b59-855f-92f32d1bdf75/what-are-those-ignorable-certificate-errors-after-the-ssl-connection-with-streamsocket-to-the-remote?forum=winappswithnativecode
                _backingStreamSocket.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                _backingStreamSocket.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
                _backingStreamSocket.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.WrongUsage);
                _backingStreamSocket.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
                _backingStreamSocket.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.RevocationInformationMissing);
                _backingStreamSocket.Control.IgnorableServerCertificateErrors.Add(ChainValidationResult.RevocationFailure);
            }
#endif

            return _backingStreamSocket
                .ConnectAsync(hn, sn, spl)
                .WrapNativeSocketExceptionsAsTask(cancellationToken);
        }

        /// <summary>
        ///     Disconnects from an endpoint previously connected to using <code>ConnectAsync</code>.
        ///     Should not be called on a <code>TcpSocketClient</code> that is not already connected.
        /// </summary>
        public Task DisconnectAsync()
        {
            return Task.Run(() =>
            {
                _backingStreamSocket.Dispose();
                _backingStreamSocket = new StreamSocket();
            });
        }

        /// </summary>
        /// <returns>The <see cref="ICommsInterface"/> which represents the interface the connection is using.</returns>
        public async Task<ICommsInterface> GetConnectedInterfaceAsync()
        {
            var interfaces = await CommsInterface.GetAllInterfacesAsync();
            return interfaces.FirstOrDefault(x => x.NativeHostName.IsEqual(_backingStreamSocket.Information.LocalAddress));
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
        
        /// <summary>
        /// Exposes the backing socket 
        /// </summary>
        public StreamSocket Socket => _backingStreamSocket;

        /// <summary>
        /// Exposes the backing socket. 
        /// </summary>
        object IExposeBackingSocket.Socket => Socket;
    }
}