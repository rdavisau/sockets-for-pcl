using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Sockets.Plugin.Abstractions;

// ReSharper disable once CheckNamespace

namespace Sockets.Plugin
{
    /// <summary>
    ///     Base class for WinRT udp socket wrapper.
    /// </summary>
    public abstract class UdpSocketBase
    {
        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Native socket field around which UdpSocketBase wraps.
        /// </summary>
        protected DatagramSocket _backingDatagramSocket;

        /// <summary>
        ///     Fired when a udp datagram has been received.
        /// </summary>
        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;

        /// <summary>
        ///     Default constructor for <code>UdpSocketBase</code>
        /// </summary>
        protected UdpSocketBase()
        {
            SetBackingSocket();
        }

        private void SetBackingSocket()
        {
            var socket = new DatagramSocket();
            socket.MessageReceived += DatagramMessageReceived;

            _backingDatagramSocket = socket;
        }

        /// <summary>
        ///     Sends the specified data to the 'default' target of the underlying DatagramSocket.
        ///     There may be no 'default' target. depending on the state of the object.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        protected Task SendAsync(byte[] data)
        {
            return SendAsync(data, data.Length);
        }

        /// <summary>
        ///     Sends the specified data to the 'default' target of the underlying DatagramSocket.
        ///     There may be no 'default' target. depending on the state of the object.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        /// <param name="length">The number of bytes from <c>data</c> to send.</param>
        public async Task SendAsync(byte[] data, int length)
        {
            var stream = _backingDatagramSocket.OutputStream.AsStreamForWrite();

            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
        }

        /// <summary>
        ///     Sends the specified data to the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="address">The remote address to which the data should be sent.</param>
        /// <param name="port">The remote port to which the data should be sent.</param>
        protected Task SendToAsync(byte[] data, string address, int port)
        {
            return SendToAsync(data, data.Length, address, port);
        }

        /// <summary>
        ///     Sends the specified data to the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="length">The number of bytes from <c>data</c> to send.</param>
        /// <param name="address">The remote address to which the data should be sent.</param>
        /// <param name="port">The remote port to which the data should be sent.</param>
        protected async Task SendToAsync(byte[] data, int length, string address, int port)
        {
            var hn = new HostName(address);
            var sn = port.ToString();

            var stream = 
                (await _backingDatagramSocket
                        .GetOutputStreamAsync(hn, sn)
                        .WrapNativeSocketExceptions())
                    .AsStreamForWrite();

            await stream.WriteAsync(data, 0, length);
            await stream.FlushAsync();
        }

        internal async void DatagramMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var remoteAddress = args.RemoteAddress.CanonicalName;
            var remotePort = args.RemotePort;
            byte[] allBytes;

            var stream = args.GetDataStream().AsStreamForRead();
            using (var mem = new MemoryStream())
            {
                await stream.CopyToAsync(mem);
                allBytes = mem.ToArray();
            }

            var wrapperArgs = new UdpSocketMessageReceivedEventArgs(remoteAddress, remotePort, allBytes);

            if (MessageReceived != null)
                MessageReceived(this, wrapperArgs);
        }

        internal Task CloseSocketAsync()
        {
            return Task.Run(() =>
            {
                _backingDatagramSocket.Dispose();
                SetBackingSocket();
            });
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
        ~UdpSocketBase()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_backingDatagramSocket != null)
                    (_backingDatagramSocket).Dispose();
            }
        }
    }
}