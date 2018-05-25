using System;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace Sockets.Plugin
{
    /// <summary>
    ///     Sends and recieves data in a udp multicast group.
    ///     Join a udp multicast address using <code>JoinMulticastGroupAsync</code>, then send data using
    ///     <code>SendMulticastAsync</code>.
    /// </summary>
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {
        /// <summary>
        ///     Joins the multicast group at the specified endpoint.
        /// </summary>
        /// <param name="multicastAddress">The address for the multicast group.</param>
        /// <param name="port">The port for the multicast group.</param>
        /// <param name="multicastOn">The <code>CommsInterface</code> to multicast on. If unspecified, all interfaces will be bound.</param>
        /// <param name="exclusive">Should address use be exclusive?</param> 
        /// <returns></returns>
        public Task JoinMulticastGroupAsync(string multicastAddress, int port, ICommsInterface multicastOn = null, bool? exclusive = null)
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
        public Task SendMulticastAsync(byte[] data)
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Sends the specified data to the multicast group, previously set using <code>JoinMulticastGroupAsync</code>.
        ///     If a group has not been set, calls will have no effect.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="length">The number of bytes from <c>data</c> to send.</param>
        public Task SendMulticastAsync(byte[] data, int length)
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }

        /// <summary>
        ///     Gets or sets the Time To Live value for the <code>UdpSocketMulticastClient</code>.
        ///     Must be called before joining a multicast group. 
        /// </summary>
        public int TTL
        {
            get { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
            set { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }

        /// <summary>
        ///     Fired when a udp datagram has been received.
        /// </summary>
        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived
        {
            add { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
            remove { throw new NotImplementedException(PCL.BaitWithoutSwitchMessage); }
        }
    }
}