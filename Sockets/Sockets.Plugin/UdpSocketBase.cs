using System;

namespace Sockets.Plugin
{
    /// <summary>
    ///     Base class for UDP socket implementations
    /// </summary>
    public abstract class UdpSocketBase
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException(PCL.BaitWithoutSwitchMessage);
        }
    }
}