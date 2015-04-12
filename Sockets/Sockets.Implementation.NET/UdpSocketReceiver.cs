using System;
using System.Net;
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
        /// <param name="port">The port to listen on. If '0', selection is delegated to the operating system.</param>        
        /// <param name="listenOn">The <code>CommsInterface</code> to listen on. If unspecified, all interfaces will be bound.</param>
        /// <returns></returns>
        public async Task StartListeningAsync(int port = 0, ICommsInterface listenOn = null)
        {
            if (listenOn != null && !listenOn.IsUsable)
                throw new InvalidOperationException("Cannot listen on an unusable interface. Check the IsUsable property before attemping to bind.");

            await Task.Run(() =>
            {
                var ip = listenOn != null ? ((CommsInterface)listenOn).NativeIpAddress : IPAddress.Any;
                var ep = new IPEndPoint(ip, port);

                _messageCanceller = new CancellationTokenSource();
                _backingUdpClient = new UdpClient(ep)
                {
                    EnableBroadcast = true
                };

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
                using (_backingUdpClient = new UdpClient { EnableBroadcast = true } )
                {
                    await base.SendToAsync(data, address, port);
                }

                // clear _backingUdpClient because it has been disposed and is unusable. 
                _backingUdpClient = null;
            }
            else
            {
                await base.SendToAsync(data, address, port);
            }

        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (_messageCanceller != null && !_messageCanceller.IsCancellationRequested)
                _messageCanceller.Cancel();

            base.Dispose();
        }
    }
}