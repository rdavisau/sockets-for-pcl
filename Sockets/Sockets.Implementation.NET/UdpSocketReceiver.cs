using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

// ReSharper disable once CheckNamespace

namespace Sockets.Plugin
{
    /// <summary>
    ///     Listens on a port for UDP traffic and can send UDP data to arbitrary endpoints.
    /// </summary>
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {
        private CancellationTokenSource _messageCanceller;

        /// <summary>
        ///     Binds the <code>UdpSocketServer</code> to the specified port on all endpoints and listens for UDP traffic.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <returns></returns>
        public async Task StartListeningAsync(int port)
        {
            await Task.Run(() =>
            {
                _messageCanceller = new CancellationTokenSource();
                _backingUdpClient = new UdpClient(port);

                RunMessageReceiver(_messageCanceller.Token);
            });
        }

        /// <summary>
        ///     Unbinds a bound <code>UdpSocketServer</code>. Should not be called if the <code>UdpSocketServer</code> has not yet
        ///     been unbound.
        /// </summary>
        public async Task StopListeningAsync()
        {
            await Task.Run(() =>
            {
                _messageCanceller.Cancel();
                _backingUdpClient.Close();
            });
        }

        /// <summary>
        ///     Sends the specified data to the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="address">The remote address to which the data should be sent.</param>
        /// <param name="port">The remote port to which the data should be sent.</param>
        public new async Task SendToAsync(byte[] data, string address, int port)
        {
            await base.SendToAsync(data, address, port);
        }
    }
}