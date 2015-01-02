using System;
using System.Threading.Tasks;
using Windows.Networking;
using Sockets.Plugin.Abstractions;

// ReSharper disable once CheckNamespace

namespace Sockets.Plugin
{
    /// <summary>
    ///     Sends and recieves data in a UDP multicast group.
    ///     Join a UDP multicast address using <code>JoinMulticastGroupAsync</code>, then send data using
    ///     <code>SendAsync</code>.
    /// </summary>
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {
        /// <summary>
        ///     Joins the multicast group at the specified endpoint.
        /// </summary>
        /// <param name="multicastAddress">The address for the multicast group.</param>
        /// <param name="port">The port for the multicast group.</param>
        /// <returns></returns>
        public async Task JoinMulticastGroupAsync(string multicastAddress, int port)
        {
            var hn = new HostName(multicastAddress);
            var sn = port.ToString();

            await _backingDatagramSocket.BindServiceNameAsync(sn);
            await Task.Run(() => _backingDatagramSocket.JoinMulticastGroup(hn));
        }

        /// <summary>
        ///     Sends the specified data to the multicast group, previously set using <code>JoinMulticastGroupAsync</code>.
        ///     If a group has not been set, calls will have no effect.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        public new async Task SendAsync(byte[] data)
        {
            await base.SendAsync(data);
        }

        /// <summary>
        ///     Removes the <code>UdpSocketMulticastClient</code> from a joined multicast group.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await CloseSocketAsync();
        }
    }
}