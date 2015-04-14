using System.Collections.ObjectModel;
using Sockets.Plugin;
using Xamarin.Forms;

namespace SocketsSample.XF
{
    public class NetworkInterfacesPage : ContentPage
    {
        private readonly ObservableCollection<CommsInterface> _netInterfaces = new ObservableCollection<CommsInterface>();
        
        public NetworkInterfacesPage()
        {
            var cellTemplate = new DataTemplate(typeof(TextCell));
            cellTemplate.SetBinding(TextCell.TextProperty, "IpAddress");
            cellTemplate.SetBinding(TextCell.DetailProperty, "Name");

            this.Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new ListView
                    {
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        ItemsSource = _netInterfaces,
                        ItemTemplate = cellTemplate
                    }
                }
            };

            LoadNetworkInterfaces();
        }

        public async void LoadNetworkInterfaces()
        {
            var interfaces = await CommsInterface.GetAllNetworkInterfacesAsync();

            foreach (var ni in interfaces)
                _netInterfaces.Add(ni);
        }
    }
}