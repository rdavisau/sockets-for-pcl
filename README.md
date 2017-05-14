# Sockets Plugin for Xamarin and Windows (PCL)

[![Join the chat at https://gitter.im/rdavisau/sockets-for-pcl](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/rdavisau/sockets-for-pcl?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

An abstraction over the socket helper classes of .NET and WinRT, providing a PCL-friendly socket library for projects targeting Xamarin iOS/Android/Forms, Xamarin.Mac, Windows Phone 8/8.1, Windows Store, UWP, and Windows Desktop. It allows you to write socket code in your PCL, simplifying cross-platform peer-to-peer communications significantly as well as enabling code sharing for many other use cases. 

This library utilises the "Bait and Switch" pattern, so must be installed via NuGet in both the PCL and your native projects. 

Get it on NuGet: `Install-Package rda.SocketsForPCL`

### Classes
The plugin currently provides the following socket abstractions:

Class|Description|.NET Abstraction|WinRT Abstraction
-----|-----------|:--------------:|:---------------:
**TcpSocketListener** | Bind to a port and accept TCP socket connections. | TcpListener | StreamSocketListener 
**TcpSocketClient** | Connect to a TCP endpoint with bi-directional communication. | TcpClient | StreamSocket
**UdpSocketReceiver** | Bind to a port and receive UDP messages. | UdpClient | DatagramSocket
**UdpSocketClient** | Send messages to arbitrary endpoints over UDP. | UdpClient | DatagramSocket
**UdpSocketMulticastClient** | Send and receive UDP messages within a multicast group. | UdpClient | DatagramSocket

Apart from the decisions made in order to merge the two APIs, the abstraction aims to be relatively unopinionated. 
This means that there is little to no protection in the library against socket failures, reliablity, retry, and other considerations.
[sockethelpers-for-pcl](https://github.com/rdavisau/sockethelpers-for-pcl) is a longer term project with the aim of providing useful functionality around the base sockets-for-pcl classes, including hub-style communications, custom protocol helpers and support for typed messaging, and error handling/life cycle and reliability options. 

### Example Usage
`TcpSocketListener` and `TcpSocketClient` each expose `ReadStream` and `WriteStream` 
properties of type `System.IO.Stream` for receiving and sending data. `UdpReceiver`, `UdpClient` and `UdpMulticastClient` expose a `MessageReceived` event and a `Send()` method due to the nature of the transport and the underlying implementations.

##### A TCP listener
```csharp
var listenPort = 11000;
var listener = new TcpSocketListener();
    
// when we get connections, read byte-by-byte from the socket's read stream
listener.ConnectionReceived += async (sender, args) => 
{
    var client = args.SocketClient; 
    
	var bytesRead = -1;
	var buf = new byte[1];
		
	while (bytesRead != 0)
	{
		bytesRead = await args.SocketClient.ReadStream.ReadAsync(buf, 0, 1);
		if (bytesRead > 0)
            Debug.Write(buf[0]);
	}
};
    
// bind to the listen port across all interfaces
await listener.StartListeningAsync(listenPort);
```
##### A TCP client
```csharp
var address = "127.0.0.1";
var port = 11000;
var r = new Random(); 
    
var client = new TcpSocketClient();
await client.ConnectAsync(address, port);
    
// we're connected!
for (int i = 0; i<5; i++)
{
    // write to the 'WriteStream' property of the socket client to send data
    var nextByte = (byte) r.Next(0,254);
    client.WriteStream.WriteByte(nextByte);
    await client.WriteStream.FlushAsync();
      
    // wait a little before sending the next bit of data
    await Task.Delay(TimeSpan.FromMilliseconds(500)); 
}
    
await client.DisconnectAsync();
```    
##### A UDP receiver
```csharp
var listenPort = 11011;
var receiver = new UdpSocketReceiver();
    
receiver.MessageReceived += (sender, args) =>
{
    // get the remote endpoint details and convert the received data into a string
    var from = String.Format("{0}:{1}", args.RemoteAddress, args.RemotePort);
    var data = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);
      
    Debug.WriteLine("{0} - {1}", from, data);
};
    
// listen for udp traffic on listenPort
await receiver.StartListeningAsync(listenPort);
```
##### A UDP client
```csharp
var port = 11011;
var address = "127.0.0.1";
    
var client = new UdpSocketClient();
    
// convert our greeting message into a byte array
var msg = "HELLO WORLD";
var msgBytes = Encoding.UTF8.GetBytes(msg);
    
// send to address:port, 
// no guarantee that anyone is there 
// or that the message is delivered.
await client.SendToAsync(msgBytes, address, port);
```
##### A multicast UDP client
```csharp    
var port = 11811;
var address = "239.192.0.1"; // must be a valid multicast address
    
// typical instantiation
var receiver = new UdpSocketMulticastClient();
receiver.TTL = 5;
    
receiver.MessageReceived += (sender, args) =>
{
    var from = String.Format("{0}:{1}", args.RemoteAddress, args.RemotePort);
    var data = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);
      
    Debug.WriteLine("{0} - {1}", from, data);
};
    
// join the multicast address:port
await receiver.JoinMulticastGroupAsync(address, port);

var msg = "HELLO MULTIVERSE";
var msgBytes = Encoding.UTF8.GetBytes(msg);
    
// send a message that will be received by all listening in
// the same multicast group. 
await receiver.SendMulticastAsync(msgBytes);
```
##### Binding to a specific interface
For a majority of mobile use cases, binding to all interfaces is a good approach. However, when working with multicast or on a machine with many interfaces, it may be useful to bind to a specific interface. The `TcpSocketListener`, `UdpSocketReceiver` and `UdpSocketMulitcastClient` classes include an optional `CommsInterface` parameter on their listen/join methods, allowing them to be bound to a specific interface only. If this parameter is not specified, all interfaces will be bound. `CommsInterface` has a static method `GetAllInterfacesAsync` that can be used to enumerate the available interfaces.
```csharp
// retrieve the list of interfaces from the device
var allInterfaces = await CommsInterface.GetAllInterfacesAsync();
    
// get the first interface with an ip address
var firstUsable = allInterfaces.FirstOrDefault(ci => ci.IsUsable);
    
if (firstUsable == null)
    return; // no connected interfaces, too bad!
        
var listener = new TcpSocketListener(); 
await listener.StartListeningAsync(11000, firstUsable); 
    
Console.WriteLine("Listening on interface with ip: {0}", firstUsable.IpAddress);
```
##### TLS Support
`TcpSocketClient` supports TLS connections (server certificate only). Pass `true` to the optional parameter `useTls` on `ConnectAsync` to enable secure communication. 

### Platform Considerations
 - On Windows Phone, you will require appropriate permissions in your app manifest. Depending on whether you are listening or sending, this could include a combination of `privateNetworkClientServer`, `internetClient` and/or  `internetClientServer` capabilities. 
 - On Windows Phone/Store, there are restrictions regarding passing traffic over loopback between separate apps (i.e. no IPC) 
 - Binding to specific interfaces is not supported on Windows Phone 8.0 (8.1 is fine). All interfaces will be bound, even if a specific `CommsInterface` is provided.
 - On Windows Phone 8.0 or Windows 8.0 the ignoreSSLErrors flag doesn't work

Additional 'higher level' features will likely end up in the [sockethelpers-for-pcl](https://github.com/rdavisau/sockethelpers-for-pcl) project mentioned earlier. 

### Contributions
Many members of the community have contributed to improving sockets-for-pcl:

 - @rdavisau
 - @jamesmontemagno (project and NuGet templates)
 - @galvesribeiro (TLS support)
 - @SatoshiARA (Mac support)
 - @aghajani (bugfixes)
 - @danielcweber (`GetStream()`, PCL-friendly `SocketException`)
 - @xen2 (configurable and consistent buffering between .NET and WinRT)
 - @NVentimiglia (bugfixes, design considerations)
 - @fubar-coder (auto/ephemeral port selection, support for getting connected interface)
 - @mattleibow (Xamarin Component Store submission)
 - @Waty (proper events, bugfixes)
 - @SparkStream (TcpSocketClient cancellation support, service name support)
 - @usasos000 (bugfixes)
 - @jasells (udpclient design considerations)
 - @vbisbest (prelease testing, bugfixes)
 - @1iveowl (udp multicast considerations)

If you have a bugfix or a feature you'd like to add, please open an issue. 
All pull requests should be opened against the `dev` branch.

### Sponsors

Thanks to Xamarin and ReSharper who provide open-source licenses for the project.
