using System;
using System.Collections;
using System.Collections.Generic;

namespace Sockets.Tests
{
    public static class PortGranter
    {
        private static readonly Dictionary<int, int> _usedPorts = new Dictionary<int,int>();
        private static Random _r = new Random();

        public static int GrantPort()
        {
            lock (((ICollection)_usedPorts).SyncRoot)
            {
                var port = -1;
                while (port == -1)
                {
                    var maybe = _r.Next(1024, 49152);
                    if (_usedPorts.ContainsKey(maybe)) continue;
                    
                    _usedPorts.Add(maybe, maybe);
                    port = maybe;
                }

                return port;
            }
        }
    }
}