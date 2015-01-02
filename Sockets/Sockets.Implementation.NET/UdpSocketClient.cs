using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

// ReSharper disable once CheckNamespace

namespace Sockets.Plugin
{
    /// <summary>
    ///     Sends UDP data to arbitrary endpoints.
    ///     If data is to be sent to a single endpoint only, use <code>ConnectAsync</code> to specify a default endpoint to
    ///     which data will be sent,
    ///     and send data with <code>SendAsync</code>.
    /// </summary>
    public class UdpSocketClient : UdpSocketBase, IUdpSocketClient
    {
        private CancellationTokenSource _messageCanceller;

        /// <summary>
        ///     Sets the endpoint at the specified address/port pair as the 'default' target of sent data.
        ///     After calling <code>ConnectAsync</code>, use <code>SendAsync</code> to send data to the default target.
        /// </summary>
        /// <param name="address">The remote address for the default target.</param>
        /// <param name="port">The remote port for the default target.</param>
        public async Task ConnectAsync(string address, int port)
        {
            _backingUdpClient = new UdpClient();
            _messageCanceller = new CancellationTokenSource();

            await Task.Run(() => _backingUdpClient.Connect(address, port));
        }

        /// <summary>
        ///     Unsets the 'default' target of sent data.
        ///     After calling <code>DisconnectAsync</code>, calls to <code>SendAsync</code> will have no effect.
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
        ///     Sends the specified data to the 'default' target of the <code>UdpSocketClient</code>, previously set using
        ///     <code>ConnectAsync</code>.
        ///     If the 'default' target has not been set, calls will have no effect.
        /// </summary>
        /// <param name="data">A byte array of data to be sent.</param>
        public new async Task SendAsync(byte[] data)
        {
            await base.SendAsync(data);
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