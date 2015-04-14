using System.Collections.Concurrent;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using Xamarin.Forms;


namespace SocketsSample.XF
{
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