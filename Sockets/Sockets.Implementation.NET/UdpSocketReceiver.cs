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
            if (_backingUdpClient == null)
            {
                // haven't bound to a port, so _backingUdpClient has not been created
                // (must be created with the binding port as a parameter, so is 
                // instantiated on call to StartListeningAsync(). If we are here, user
                // is sending before having 'bound' to a port, so just create a temporary
                // backing client to send this data. 
                using (_backingUdpClient = new UdpClient())
                {
                    await base.SendToAsync(data, address, port);
                }
            }
            else
            {
                await base.SendToAsync(data, address, port);
            }

        }
    }
}