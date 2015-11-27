﻿using System;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace Sockets.Plugin
{
    /// <summary>
    ///     Binds to a port and listens for TCP connections.
    ///     Use <code>StartListeningAsync</code> to bind to a local port, then handle <code>ConnectionReceived</code> events as
    ///     clients connect.
    /// </summary>
    public class TcpSocketListener : ITcpSocketListener
    {
        /// <summary>
        ///     Default constructor for <code>TcpSocketListener</code>.
        /// </summary>
        public TcpSocketListener()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocketListener"/> class.
        /// </summary>
        /// <param name="bufferSize">Size of the buffer for the write stream to any connected sockets.</param>
        public TcpSocketListener(int bufferSize)
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Binds the <code>TcpSocketListener</code> to the specified port on all endpoints and listens for TCP connections.
        /// </summary>
        /// <param name="port">The port to listen on. If '0', selection is delegated to the operating system.</param>
        /// <param name="listenOn">The <code>CommsInterface</code> to listen on. If unspecified, all interfaces will be bound.</param>
        /// <returns></returns>
        public Task StartListeningAsync(int port, ICommsInterface listenOn = null)
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Stops the <code>TcpSocketListener</code> from listening for new TCP connections.
        ///     This does not disconnect existing connections.
        /// </summary>
        public Task StopListeningAsync()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     The port to which the TcpSocketListener is currently bound
        /// </summary>
        public int LocalPort
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        ///     Fired when a new TCP connection has been received.
        ///     Use the <code>SocketClient</code> property of the <code>TcpSocketListenerConnectEventArgs</code>
        ///     to get a <code>TcpSocketClient</code> representing the connection for sending and receiving data.
        /// </summary>
        public event EventHandler<TcpSocketListenerConnectEventArgs> ConnectionReceived
        {
            add { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
            remove { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
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