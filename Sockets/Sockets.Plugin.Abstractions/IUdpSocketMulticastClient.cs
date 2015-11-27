using System;
using System.Threading.Tasks;

namespace Sockets.Plugin.Abstractions
{
    /// <summary>
    ///     Sends and recieves data in a udp multicast group.
    ///     Join a udp multicast address using <code>JoinMulticastGroupAsync</code>, then send data using
    ///     <code>SendMulticastAsync</code>.
    /// </summary>
    public interface IUdpSocketMulticastClient : IDisposable, IUdpMessageProvider
    {
        /// <summary>
        ///     Joins the multicast group at the specified endpoint.
        /// </summary>
        /// <param name="multicastAddress">The address for the multicast group.</param>
        /// <param name="port">The port for the multicast group.</param>        
        /// <param name="multicastOn">The <code>CommsInterface</code> to multicast on. If unspecified, all interfaces will be bound.</param>
        // <returns></returns>
        Task JoinMulticastGroupAsync(string multicastAddress, int port, ICommsInterface multicastOn);

        /// <summary>
        ///     Removes the <code>UdpSocketMulticastClient</code> from a joined multicast group.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        ///     Sends the specified data to the multicast group, previously set using <code>JoinMulticastGroupAsync</code>.
        ///     If a group has not been set, calls will have no effect.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        Task SendMulticastAsync(byte[] data);

        /// <summary>
        ///     Gets or sets the Time To Live value for the <code>UdpSocketMulticastClient</code>.
        ///     Must be called before joining a multicast group. 
        /// </summary>
        int TTL { get; set; }

        ///// <summary>
        /////     Fired when a udp datagram has been received.
        ///// </summary>
        //EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived { get; set; }
    }
}