using System;
using System.Threading.Tasks;

namespace Sockets.Plugin.Abstractions
{
    /// <summary>
    ///     Listens on a port for UDP traffic and can send UDP data to arbitrary endpoints.
    /// </summary>
    public interface IUdpSocketReceiver
    {
        /// <summary>
        ///     Binds the <code>UdpSocketReceiver</code> to the specified port on all endpoints and listens for UDP traffic.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <returns></returns>
        Task StartListeningAsync(int port);

        /// <summary>
        ///     Unbinds a bound <code>UdpSocketReceiver</code>. Should not be called if the <code>UdpSocketReceiver</code> has not
        ///     yet been unbound.
        /// </summary>
        Task StopListeningAsync();

        /// <summary>
        ///     Sends the specified data to the endpoint at the specified address/port pair.
        /// </summary>
        /// <param name="data">A byte array of data to send.</param>
        /// <param name="address">The remote address to which the data should be sent.</param>
        /// <param name="port">The remote port to which the data should be sent.</param>
        Task SendToAsync(byte[] data, string address, int port);

        /// <summary>
        ///     Fired when a UDP datagram has been received.
        /// </summary>
        EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived { get; set; }
    }
}