using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
        private CancellationTokenSource _messageCanceller;

        /// <summary>
        ///     Joins the multicast group at the specified endpoint.
        /// </summary>
        /// <param name="multicastAddress">The address for the multicast group.</param>
        /// <param name="port">The port for the multicast group.</param>
        /// <returns></returns>
        public async Task JoinMulticastGroupAsync(string multicastAddress, int port)
        {
            var ip = IPAddress.Parse(multicastAddress);
            _backingUdpClient = new UdpClient(port);
            _messageCanceller = new CancellationTokenSource();

            await Task.Run(() => _backingUdpClient.JoinMulticastGroup(ip));

            RunMessageReceiver(_messageCanceller.Token);
        }

        /// <summary>
        ///     Removes the <code>UdpSocketMulticastClient</code> from a joined multicast group.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await Task.Run(() =>
            {
                _messageCanceller.Cancel();
                _backingUdpClient.Close();
            });
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
    }
}