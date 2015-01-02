using System;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace Sockets.Plugin
{
    /// <summary>
    ///     Sends udp data to arbitrary endpoints.
    ///     If data is to be sent to a single endpoint only, use <code>ConnectAsync</code> to specify a default endpoint to
    ///     which data will be sent,
    ///     and send data with <code>SendAsync</code>.
    /// </summary>
    public class UdpSocketClient : IUdpSocketClient
    {
        /// <summary>
        ///     Sets the endpoint at the specified address/port pair as the 'default' target of sent data.
        ///     After calling <code>ConnectAsync</code>, use <code>SendAsync</code> to send data to the default target.
        /// </summary>
        /// <param name="address">The remote address for the default target.</param>
        /// <param name="port">The remote port for the default target.</param>
        public Task ConnectAsync(string address, int port)
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Unsets the 'default' target of sent data.
        ///     After calling <code>DisconnectAsync</code>, calls to <code>SendAsync</code> will have no effect.
        /// </summary>
        public Task DisconnectAsync()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Sends the specified data to the 'default' target of the <code>UdpSocketClient</code>, previously set using
        ///     <code>ConnectAsync</code>.
        ///     If the 'default' target has not been set, calls will have no effect.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        public Task SendAsync(byte[] data)
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Sends the specified data to the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="address">The remote address to which the data should be sent.</param>
        /// <param name="port">The remote port to which the data should be sent.</param>
        public Task SendToAsync(byte[] data, string address, int port)
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Fired when a udp datagram has been received.
        /// </summary>
        public EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
            set { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }
    }
}