using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sockets.Plugin;
using Xunit;

namespace Sockets.Tests
{
    public class CommsInterfaceTests
    {
        [Fact]
        public async Task CommsInterface_CanEnumerateNetworkInterfaces()
        {
            var timeout =
                Task.Delay(TimeSpan.FromSeconds(3))
                    .ContinueWith(_ => { throw new TimeoutException("Couldn't enumerate network interfaces."); });
            var ifs = CommsInterface.GetAllInterfacesAsync();

            var result = await Task.WhenAny(timeout, ifs);

            if (result.Exception != null)
                throw result.Exception;

            Debug.WriteLine("{0} interfaces returned", ifs.Result.Count);
        }

        [Fact]
        public async Task CommsInterface_CanAccessInterfaceProperties()
        {
            var ifs = 
                (await CommsInterface.GetAllInterfacesAsync())
                    .Select(i=> String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", 
                                i.Name, i.IpAddress, i.ConnectionStatus,
                                i.NativeInterfaceId, i.IsLoopback, i.IsUsable, 
                                i.GatewayAddress, i.BroadcastAddress));

            Debug.WriteLine(String.Join(Environment.NewLine, ifs));
        }


    }
}