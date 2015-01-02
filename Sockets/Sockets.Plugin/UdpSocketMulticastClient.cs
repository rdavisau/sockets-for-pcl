using System;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace Sockets.Plugin
{
    /// <summary>
    ///     Sends and recieves data in a udp multicast group.
    ///     Join a udp multicast address using <code>JoinMulticastGroupAsync</code>, then send data using
    ///     <code>SendAsync</code>.
    /// </summary>
    public class UdpSocketMulticastClient : IUdpSocketMulticastClient
    {
        /// <summary>
        ///     Joins the multicast group at the specified endpoint.
        /// </summary>
        /// <param name="multicastAddress">The address for the multicast group.</param>
        /// <param name="port">The port for the multicast group.</param>
        /// <returns></returns>
        public Task JoinMulticastGroupAsync(string multicastAddress, int port)
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Removes the <code>UdpSocketMulticastClient</code> from a joined multicast group.
        /// </summary>
        public Task DisconnectAsync()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Sends the specified data to the multicast group, previously set using <code>JoinMulticastGroupAsync</code>.
        ///     If a group has not been set, calls will have no effect.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        public Task SendAsync(byte[] data)
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