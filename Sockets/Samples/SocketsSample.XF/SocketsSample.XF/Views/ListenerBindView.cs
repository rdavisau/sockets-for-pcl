using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SocketsSample.XF
{
    /// <summary>
    /// A stacklayout with ip, port and connect controls, and callbacks for when connected or disconnected is tapped
    /// </summary>
    public class ListenerBindView : ContentView
    {
        public Func<int, Task<bool>> StartListeningTapped { get; set; }
        public Func<Task> StopListeningTapped { get; set; }
        private readonly Page _parentPage;

        private bool _listening;

        public ListenerBindView(int defaultPort, Page parentPage)
        {
            _parentPage = parentPage;

            var portEntry = new Entry()
            {
                Placeholder = "Listen Port",
                Text = defaultPort.ToString(),
                HorizontalOptions = LayoutOptions.Start
            };

            var listenButton = new Button
            {
                Text = "Start Listening",
            };


            // listen / stop listening 
            listenButton.Clicked += async (sender, args) => 
            {
                // if not already listening
                if (!_listening)
                {
                    if (StartListeningTapped == null) return;

                    // check valid port
                    var port = -1;
                    var isNumeric = Int32.TryParse(portEntry.Text, out port);

                    if (!isNumeric || ((port < 1001 || port > 65535)))
                    {
                        await _parentPage.DisplayAlert("Invalid Port", "Port must be numeric value between 1001 & 65535", "My bad");
                        return;
                    }

                    // callback
                    if (await StartListeningTapped(port))
                    {
                        _listening = true;
                        listenButton.Text = "Stop Listening";
                    };
                }
                else
                {
                    if (StopListeningTapped == null) return;

                    // callback
                    await StopListeningTapped();
                    _listening = false;
                    listenButton.Text = "Start Listening";
                }
                
            };

            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new Label { 
                        Text = "Listen port:", 
                        VerticalOptions = LayoutOptions.CenterAndExpand 
                    },
                    portEntry, 
                    listenButton
                }
            };
        }
    }
}