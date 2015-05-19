_This changelog refers to nuget package releases._

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
