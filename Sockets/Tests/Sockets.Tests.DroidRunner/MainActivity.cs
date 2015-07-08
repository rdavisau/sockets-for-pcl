using System;
using System.Reflection;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Xunit.Sdk;
using Xunit.Runners.UI;

namespace Sockets.Tests.DroidRunner
{
    [Activity(Label = "xUnit Android Runner", MainLauncher = true, Theme= "@android:style/Theme.Material.Light")]
    public class MainActivity : RunnerActivity
    {

        protected override void OnCreate(Bundle bundle)
        {
            // tests can be inside the main assembly
			AddTestAssembly(typeof(Sockets.Tests.TcpSocketClientTests).Assembly);
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);

            base.OnCreate(bundle);
        }
    }
}

