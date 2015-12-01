using System;

namespace Sockets.Plugin.Abstractions
{
    public sealed class SocketException : Exception
    {
        public SocketException(Exception innerException) : base(innerException.Message, innerException)
        {

        }
    }
}