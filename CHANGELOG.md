_This changelog refers to nuget package releases._

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
  
  - Fixed a NullReferenceException in the .NET abstraction that would occur if a ````UdpSocketReceiver````'s ````SendToAsync```` method was called before ````StartListeningAsync````.

#### 1.0.0.0 (2014-12-03)

First published to NuGet. 
