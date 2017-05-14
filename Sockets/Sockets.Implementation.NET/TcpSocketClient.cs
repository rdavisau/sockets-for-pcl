﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Sockets.Plugin.Abstractions.Resources;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = Sockets.Plugin.Abstractions.SocketException;

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
        private TcpClient _backingTcpClient;
        private readonly int _bufferSize;
        private SslStream _secureStream;
        private Stream _writeStream;

        /// <summary>
        ///     Default constructor for <code>TcpSocketClient</code>.
        /// </summary>
        public TcpSocketClient()
        {
            _backingTcpClient = new TcpClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocketClient"/> class.
        /// </summary>
        /// <param name="bufferSize">Size of the buffer for the write stream.</param>
        public TcpSocketClient(int bufferSize) : this()
        {
            _bufferSize = bufferSize;
        }

        internal TcpSocketClient(TcpClient backingClient, int bufferSize)
        {
            _backingTcpClient = backingClient;
            _bufferSize = bufferSize;
            InitializeWriteStream();
        }

        /// <summary>
        ///     Establishes a TCP connection with the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="address">The address of the endpoint to connect to.</param>
        /// <param name="port">The port of the endpoint to connect to.</param>
        /// <param name="secure">True to enable TLS on the socket.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        public async Task ConnectAsync(string address, int port, bool secure = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            // standard connect
            var connectTask =
                _backingTcpClient
                    .ConnectAsync(address, port)
                    .WrapNativeSocketExceptions();

            // set up cancellation trigger
            var ret = new TaskCompletionSource<bool>();
            var canceller = cancellationToken.Register(() => ret.SetCanceled());

            // if cancellation comes before connect completes, we honour it
            var okOrCancelled = await Task.WhenAny(connectTask, ret.Task);

            if (okOrCancelled == ret.Task)
            {
#pragma warning disable CS4014
                // ensure we observe the connectTask's exception in case downstream consumers throw on unobserved tasks
                connectTask.ContinueWith(t => $"{t.Exception}", TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CS4014 

                // reset the backing field.
                // depending on the state of the socket this may throw ODE which it is appropriate to ignore
                try { await DisconnectAsync(); } catch (ObjectDisposedException) { }

                // notify that we did cancel
                cancellationToken.ThrowIfCancellationRequested();
            }
            else
                canceller.Dispose();

            if (okOrCancelled.IsFaulted)
                throw okOrCancelled.Exception.InnerException;

            InitializeWriteStream();

            if (secure)
            {
                var secureStream = new SslStream(_writeStream, true, (sender, cert, chain, sslPolicy) => ServerValidationCallback(sender, cert, chain, sslPolicy));
                secureStream.AuthenticateAsClient(address, null, System.Security.Authentication.SslProtocols.Tls, false);
                _secureStream = secureStream;
            }            
        }

        /// <summary>
        ///     Establishes a TCP connection with the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="address">The address of the endpoint to connect to.</param>
        /// <param name="service">The service name of the endpoint to connect to.</param>
        /// <param name="secure">True to enable TLS on the socket.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        public Task ConnectAsync(string address, string service, bool secure = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var port = ServiceNames.PortForTcpServiceName(service);
            return ConnectAsync(address, port, secure, cancellationToken);
        }

        #region Secure Sockets Details
        
        private bool ServerValidationCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            switch (sslPolicyErrors)
            {
                case SslPolicyErrors.RemoteCertificateNameMismatch:
                    return false;
                case SslPolicyErrors.RemoteCertificateNotAvailable:
                    return false;
                case SslPolicyErrors.RemoteCertificateChainErrors:
                    return false;
            }
            return true;
        }

        #endregion

        /// <summary>
        ///     Disconnects from an endpoint previously connected to using <code>ConnectAsync</code>.
        ///     Should not be called on a <code>TcpSocketClient</code> that is not already connected.
        /// </summary>
        public Task DisconnectAsync()
        {
            return Task.Run(() => {
                _backingTcpClient.Close();
                _secureStream = null;
                _backingTcpClient = new TcpClient();
            });
        }

        /// <summary>
        /// Gets the interface the connection is using.
        /// </summary>
        /// <returns>The <see cref="ICommsInterface"/> which represents the interface the connection is using.</returns>
        public async Task<ICommsInterface> GetConnectedInterfaceAsync()
        {
            var ipEndpoint = (IPEndPoint)_backingTcpClient.Client.LocalEndPoint;
            var interfaces = await CommsInterface.GetAllInterfacesAsync();
            return interfaces.FirstOrDefault(x => x.NativeIpAddress.Equals(ipEndpoint.Address));
        }

        /// <summary>
        ///     A stream that can be used for receiving data from the remote endpoint.
        /// </summary>
        public Stream ReadStream
        {
            get
            {
                if (_secureStream != null)
                {
                    return _secureStream as Stream;
                }
                return _backingTcpClient.GetStream();
            }
        }

        /// <summary>
        ///     A stream that can be used for sending data to the remote endpoint.
        /// </summary>
        public Stream WriteStream
        {
            get
            {
                if (_secureStream != null)
                {
                    return _secureStream as Stream;
                }
                return _writeStream;
            }
        }

        private IPEndPoint RemoteEndpoint
        {
            get
            {
                try
                {
                    return _backingTcpClient.Client.RemoteEndPoint as IPEndPoint;
                }
                catch(PlatformSocketException ex)
                {
                    throw new PclSocketException(ex);
                }
            }
        }

        /// <summary>
        ///     The address of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public string RemoteAddress
        {
            get { return RemoteEndpoint.Address.ToString(); }
        }

        /// <summary>
        ///     The port of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public int RemotePort
        {
            get { return RemoteEndpoint.Port; }
        }

        /// <summary>
        /// Enables or disables delay when send or receive buffers are full.
        /// </summary>
        public bool NoDelay
        {
            get { return _backingTcpClient.NoDelay; }
            set { _backingTcpClient.NoDelay = value; }
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

        private void InitializeWriteStream()
        {
            _writeStream = _bufferSize != 0 ? (Stream)new BufferedStream(_backingTcpClient.GetStream(), _bufferSize) : _backingTcpClient.GetStream();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_writeStream != null)
                    _writeStream.Dispose();
                if (_backingTcpClient != null)
                    ((IDisposable)_backingTcpClient).Dispose();
            }
        }
        
        /// <summary>
        /// Exposes the backing socket.
        /// </summary>
        public TcpClient Socket => _backingTcpClient;

        /// <summary>
        /// Exposes the backing socket. 
        /// </summary>
        object IExposeBackingSocket.Socket => Socket;
    }
}