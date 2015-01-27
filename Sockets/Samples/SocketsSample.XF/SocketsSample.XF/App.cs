using System.Collections.Concurrent;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using Sockets.Plugin;
using SocketsSample.XF;
using Xamarin.Forms;


namespace SocketsSample.XF
{
    public static class Global
    {
        public static CommsInterface DefaultCommsInterface { get; set; }
    }

    public class App : Application
    {
        public App()
        {
            // The root page of your application
            var tabPage = new TabbedPage();
            var menuPage = new MenuPage(tabPage);

            tabPage.Children.Add(menuPage);

            MainPage = tabPage;
        }
    }
}