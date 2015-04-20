using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SocketsSample.XF
{
    public class MenuPage : ContentPage
    {
        private TabbedPage _parentTabPage;

        public MenuPage(TabbedPage parentTabPage)
        {
            _parentTabPage = parentTabPage;

            Title = "Classes";

            Init();
        }

        private async void Init()
        {
            var samples = new[]
            {
                typeof (TcpSocketListenerPage),
                typeof (TcpSocketClientPage),
                typeof (UdpSocketReceiverPage),
                typeof (UdpSocketClientPage),
                typeof (UdpSocketMulticastClientPage),
            };

            var netCell = new TextCell() {Text = "View Network Interfaces "};
            netCell.Tapped += (sender, args) =>
            {
                var netPage = new NetworkInterfacesPage()
                {
                    Title = "Network Interfaces"
                };

                _parentTabPage.Children.Add(netPage);
            };

            var cells = await Task.Run(() => samples.Select(t =>
            {
                var title = t.Name.Substring(0, t.Name.IndexOf("Page", StringComparison.Ordinal));

                var cell = new TextCell()
                {
                    Text = String.Format("Add {0}", title)
                };

                cell.Tapped += (sender, args) =>
                {
                    var page = (ContentPage)Activator.CreateInstance(t);
                    page.Title = title;
                    _parentTabPage.Children.Add(page);
                };

                return cell;

            })
                .Concat(new[]
                {
                    netCell
                    // more cells here, one day . . . 
                })            
                .ToList());

            var tableRoot = new TableRoot() {new TableSection("Classes") { cells } };
            var tableView = new TableView(tableRoot);

			this.Content = new StackLayout {
				Padding = new Thickness (0, Device.OnPlatform (20, 0, 0), 0, 0),
				Children = { 
					tableView
				}
			};
        }
    }
}