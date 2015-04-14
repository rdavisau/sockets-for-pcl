using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SocketsSample.XF
{
    /// <summary>
    /// A stacklayout with ip, port and connect controls, and callbacks for when connected or disconnected is tapped
    /// </summary>
    public class ClientConnectView : ContentView
    {
        /// <summary>
        /// ConnectedTapped should return true if the connection action was successful.
        /// </summary>
        public Func<string, int, Task<bool>> ConnectTapped { get; set; }
        public Func<Task> DisconnectTapped { get; set; }
        private readonly Page _parentPage;

        private bool _connected;

        public ClientConnectView(string defaultAddress, int defaultPort, Page parentPage)
        {
            _parentPage = parentPage;

            var addressEntry = new Entry
            {
                Placeholder = "Address",
                Text = defaultAddress,
                HorizontalOptions = LayoutOptions.StartAndExpand
            };

            var portEntry = new Entry()
            {
                Placeholder = "Port",
                Text = defaultPort.ToString(),
                HorizontalOptions = LayoutOptions.Start
            };

            var connectButton = new Button
            {
                Text = "Connect",
            };


            // connect / disconnect
            connectButton.Clicked += async (sender, args) => 
            {
                // if not already connected
                if (!_connected)
                {
                    if (ConnectTapped == null) return;

                    var address = addressEntry.Text;
                    // check valid port
                    var port = -1;
                    var isNumeric = Int32.TryParse(portEntry.Text, out port);

                    if (!isNumeric || ((port < 1001 || port > 65535)))
                    {
                        await _parentPage.DisplayAlert("Invalid Port", "Port must be numeric value between 1001 & 65535", "My bad");
                        return;
                    }

                    // callback
                    if (await ConnectTapped(address, port))
                    {
                        _connected = true;
                        connectButton.Text = "Disconnect";
                    };
                }
                else
                {
                    if (DisconnectTapped == null) return;

                    // callback
                    await DisconnectTapped();
                    _connected = false;
                    connectButton.Text = "Start Listening";
                }
                
            };

            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new Label { 
                        Text = "Connect to:", 
                        VerticalOptions = LayoutOptions.CenterAndExpand 
                    },
                    addressEntry,
                    portEntry, 
                    connectButton
                }
            };
        }
    }
}