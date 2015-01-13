﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sockets.Plugin.Abstractions
{
    /// <summary>
    /// Provides a summary of an available network interface on the device.
    /// </summary>
    public interface ICommsInterface
    {
        /// <summary>
        /// The interface identifier provided by the underlying platform.
        /// </summary>
        string NativeInterfaceId { get; }

        /// <summary>
        /// The interface name, as provided by the underlying platform.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The IPv4 Address of the interface, if connected. 
        /// </summary>
        string IpAddress { get; }

        /// <summary>
        /// The IPv4 address of the gateway, if available.
        /// </summary>
        string GatewayAddress { get; }

        /// <summary>
        /// The IPv4 broadcast address for the interface, if available.
        /// </summary>
        string BroadcastAddress { get; }

        /// <summary>
        /// The connection status of the interface, if available
        /// </summary>
        NetworkInterfaceStatus ConnectionStatus { get; }

        /// <summary>
        /// Indicates whether the interface has a network address and can be used for 
        /// sending/receiving data.
        /// </summary>
        bool IsUsable { get; }

        /// <summary>
        /// Indicates whether the interface is the loopback interface
        /// </summary>
        bool IsLoopback { get; }
    }
}
