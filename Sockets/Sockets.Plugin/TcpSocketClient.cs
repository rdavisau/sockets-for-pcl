﻿using System;
using System.IO;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

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
        /// <summary>
        ///     Default constructor for <code>TcpSocketClient</code>.
        /// </summary>
        public TcpSocketClient()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocketClient"/> class.
        /// </summary>
        /// <param name="bufferSize">Size of the buffer for the write stream.</param>
        public TcpSocketClient(int bufferSize) : this()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
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
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Establishes a TCP connection with the service at the specified address/port pair.
        /// </summary>
        /// <param name="address">The address of the endpoint to connect to.</param>
        /// <param name="service">The service of the endpoint to connect to.</param>
        /// <param name="secure">True to enable TLS on the socket.</param>
        /// <param name="timeout">Client specified timout.</param>
        public Task ConnectAsync(string address, string service, bool secure = false, int timeout = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Disconnects from an endpoint previously connected to using <code>ConnectAsync</code>.
        ///     Should not be called on a <code>TcpSocketClient</code> that is not already connected.
        /// </summary>
        public Task DisconnectAsync()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Returns the underlying backingField.
        /// </summary>
        public object Socket
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     A stream that can be used for receiving data from the remote endpoint.
        /// </summary>
        public Stream ReadStream
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        ///     A stream that can be used for sending data to the remote endpoint.
        /// </summary>
        public Stream WriteStream
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        ///     The address of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public string RemoteAddress
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        ///     The port of the remote endpoint to which the <code>TcpSocketClient</code> is currently connected.
        /// </summary>
        public int RemotePort
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }
    }
}