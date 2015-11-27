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
    public abstract class UdpSocketBase : IUdpMessageProvider
    {
        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Native socket field around which UdpSocketBase wraps.
        /// </summary>
        protected DatagramSocket _backingDatagramSocket;

        /// <summary>
        ///     Fired when a UDP datagram has been received.
        /// </summary>
        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived
        {
            add { rxHandlers += value; }
            remove { rxHandlers -= value; }
        }
        /// <summary>
        /// This private member coupled with the public property follows the 
        /// .NET event pattern allowing auto-complete to work in Visual studio,
        /// and other editors (Xamarin).  Code written to the old API will
        /// still work, unless direct assungment was used.
        /// 
        /// ex: var udp = new UdpCLient();
        /// usp.MessageReceived += DataHandler;//now auto complete works here.
        /// </summary>
        private EventHandler<UdpSocketMessageReceivedEventArgs> rxHandlers;

        /// <summary>
        /// raises the event if there are any subscribers
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessageReceived(UdpSocketMessageReceivedEventArgs e)
        {
            if (rxHandlers != null)
                rxHandlers(this, e);
        }

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
        protected async Task SendAsync(byte[] data)
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
        protected async Task SendToAsync(byte[] data, string address, int port)
        {
            var hn = new HostName(address);
            var sn = port.ToString();

            var stream = (await _backingDatagramSocket.GetOutputStreamAsync(hn, sn)).AsStreamForWrite();

            await stream.WriteAsync(data, 0, data.Length);
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

            OnMessageReceived( wrapperArgs);
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