using System;
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

            var netInterfacesListView = new ListView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                ItemsSource = _netInterfaces,
                ItemTemplate = cellTemplate
            };

            netInterfacesListView.ItemSelected += async (sender, args) =>
            {
                var selectedInterface = args.SelectedItem as CommsInterface;

                var action = await DisplayActionSheet("Set as default interface?", "Cancel", null, "Yes");

                if (action == "Yes")
                {
                    Global.DefaultCommsInterface = selectedInterface;

                    DisplayAlert("Default Interface Set", String.Format("Interface with ip {0} will be used for all subsequent bindings.",
                            selectedInterface.IpAddress), "Sweet!");
                }
                else
                {
                    Global.DefaultCommsInterface = null;
                    DisplayAlert("Default Interface Unset", "Subsequent bindings will occur across all interfaces.", "Cool, probably for the best.");
                }
            };

            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    netInterfacesListView
                },
				Padding = new Thickness(0, Device.OnPlatform(20,0,0), 0, 0)
            };

            LoadNetworkInterfaces();
        }

        public async void LoadNetworkInterfaces()
        {
            var interfaces = await CommsInterface.GetAllInterfacesAsync();

            foreach (var ni in interfaces)
                _netInterfaces.Add(ni);
        }
    }
}