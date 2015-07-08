using System;
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
        /// <summary>
        ///     Binds the <code>UdpSocketReceiver</code> to the specified port on all endpoints and listens for UDP traffic.
        /// </summary>
        /// <param name="port">The port to listen on. If '0', selection is delegated to the operating system.</param>        
        /// <param name="listenOn">The <code>CommsInterface</code> to listen on. If unspecified, all interfaces will be bound.</param>
        /// <returns></returns>
        public Task StartListeningAsync(int port = 0, ICommsInterface listenOn = null)
        {
            if (listenOn != null && !listenOn.IsUsable)
                throw new InvalidOperationException("Cannot listen on an unusable interface. Check the IsUsable property before attemping to bind.");
            
            var sn = port == 0 ? "" : port.ToString();
#if !WP80    
            if (listenOn != null)
            {
                var adapter = ((CommsInterface) listenOn).NativeNetworkAdapter;
                return _backingDatagramSocket
                            .BindServiceNameAsync(sn, adapter)
                            .AsTask()
                            .WrapNativeSocketExceptions();
            }
            else
#endif
                return _backingDatagramSocket
                            .BindServiceNameAsync(sn)
                            .AsTask()
                            .WrapNativeSocketExceptions();
        }

        /// <summary>   
        ///     Unbinds a bound <code>UdpSocketReceiver</code>. Should not be called if the <code>UdpSocketReceiver</code> has not
        ///     yet been unbound.
        /// </summary>
        public Task StopListeningAsync()
        {
            return CloseSocketAsync();
        }

        /// <summary>
        ///     Sends the specified data to the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="address">The remote address to which the data should be sent.</param>
        /// <param name="port">The remote port to which the data should be sent.</param>
        public new Task SendToAsync(byte[] data, string address, int port)
        {
            return SendToAsync(data, address, port)
                    .WrapNativeSocketExceptions();
        }
    }
}