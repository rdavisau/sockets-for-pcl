_This changelog refers to nuget package releases._

#### 2.0.0 (2016-01-11)

Deprecations:

- Support for the iOS "Classic" API has been removed

Features: 

- A `SocketException` class has been added, allowing socket-related exceptions to be caught from PCL code. Thanks @danielcweber
- It is now possible to retrieve the `ICommsInterface` that a `TcpSocketClient` is connected via. Thanks @fubar-coder
- Auto-properties have been replaced with proper events across all classes. Thanks @Waty
- `TcpSocketClient`'s `ConnectAsync` method optionally takes a `CancellationToken` to support client-invoked cancellation and scenarios like timeout. Thanks @SparkStream
- You may now specify a service name rather than port when connecting with a `TcpSocketClient`. Thanks @SparkStream
- For those that need it, you can access the underlying .NET or WinRT/UWP socket instance from native (non-PCL) projects. Thanks @SparkStream
- `UdpSocketClient` can now receive response packets. This simplifies scenarios where you don't need to listen for new packets, but do need to receive response packets. Thanks @SatoshiARA
- All the Udp socket classes now include a `Send-` overload that allows you to specify the number of bytes to be read from the input. This can avoid the need to duplicate a buffer when working with streams. Thanks @jasells

Bugfixes: 

- Fixed `InvalidCastException` being thrown by `TcpSocketListener.Dispose()`. Thanks @Waty
- Fix for a rare issue where the Udp classes could throw an exception after receiving an ICMP unreachable packet in certain cases

Other:

- sockets-for-pcl can now also be installed via the Xamarin Component Store. Thanks @mattleibow

#### 1.2.2 (2015-07-27)

Features: 

- `TcpSocketClient` and `TcpSocketListener` now supports setting of the buffer size used when sending data. If unset, this defaults to zero (unbuffered). This also addresses an inconsistency in default buffer sizes between .NET platforms and WinRT platforms. Thanks @xen2
- `TcpSocketListener` now supports os-based/ephemeral port selection. When passing `0` to the `port` parameter of `StartListeningAsync`, the selection of bound port is deferred to the operating system. You can determine what port was bound by checking the `LocalPort` property. 
      
#### 1.2.1 (2015-06-10)

Bugfixes:

- Fixed incorrect assembly version for WP8.

#### 1.2.0 (2015-05-20)

Features:

- Support for MonoMac, Xamarin.Mac Classic and Xamarin.Mac Unified. Thanks @NewtonARA.
- Extension method `GetStream` added to `ITcpSocketClient`, easing migration from .NET projects. Thanks @danielcweber.

Other:

- sockets-for-pcl now comes with LINQPad samples! When you add the package to a LINQPad query for the first time, a set of samples will be added to your Samples tab, demonstrating typical usage. 
- Will be trying Real Hard to do SemVer properly from now on.

#### 1.1.8 (2015-04-20)

Bugfixes:

- Fixed 'A method was called at an unexpected time' exception when that occured when disconnecting a `UdpSocketMulticastClient` on WinRT. Thanks @aghajani for discovering.

Other: 

- Removed unneccessary `async` modifiers and `await` calls.

#### 1.1.6 / 1.1.7 (2015-04-12)

Features:

- `UdpSocketReceiver` can now be bound to a port chosen by the operating system. Pass `0` to the `port` argument of `StartListeningAsync`. 

Bugfixes:

- Calling `Dispose` on a bound `UdpSocketReceiver` or `UdpSocketMulticastClient` now cancels the internal message reading loop.

#### 1.1.5 (2015-03-11)

Bugfixes:

- Fixed a `NullReferenceException` that could occur in `CommsInterface.GetAllInterfacesAsync` if an interface had no IPv4 unicast address. 

#### 1.1.4 (2015-02-09)

Features:

- `TcpSocketClient` supports TLS connections via optional parameter on `ConnectAsync`. Thanks @galvesribeiro.

#### 1.1.2 / 1.1.3 (2015-02-06)

Bugfixes:

- The backing `UdpClient` instances for the .NET UdpSocket* classes now have the `EnableBroadcast` property set to true. This should prevent `Access Denied` exceptions from occuring when attempting to send data to a broadcast address.  
- Fixed an `ObjectDisposedException` that would occur when `SendToAsync` was called a second time on a `UdpSocketReceiver` if it had not yet been bound using 'StartListeningAsync`.

#### 1.1.1 (2015-01-30)

Other:
  
  - Fixed an error in the .nuspec that resulted in `Sockets.Plugin.Abstractions` not being available under Windows Desktop. Whoops :shipit:.

#### 1.1.0 (2015-01-27)

Features:
  
  - Added ````CommsInterface```` class that abstracts over native network interface representations. Use ````GetAllInterfacesAsync```` to retrieve the available interfaces.
  - Added optional parameter to listen methods that allows a specific ````CommsInterface```` to be bound. Ignored on WP8.0. 

Other:

  - Now with ````ERR DIV BY ZERO````% more SemVer. 


#### 1.0.0.1 (2014-12-04)

Bugfixes:
  
  - Fixed a `NullReferenceException` in the .NET abstraction that would occur if a ````UdpSocketReceiver````'s ````SendToAsync```` method was called before ````StartListeningAsync````.

#### 1.0.0.0 (2014-12-03)

First published to NuGet. 
