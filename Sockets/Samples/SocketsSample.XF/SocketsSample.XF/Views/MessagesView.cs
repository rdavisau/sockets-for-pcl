using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SocketsSample.XF
{
    /// <summary>
    /// A stacklayout with listview for sent/received messages and (optionally) a text area for messages to send and a callback 
    /// to the parent for when send is clicked. It subscribes to the <code>messagesObservable</code> passed in the constructor
    /// which provides it with received message data.
    /// </summary>
    public class MessagesView : ContentView
    {
        public Func<string, Task<Message>> SendData { get; set; }
        private readonly bool _showSendArea;
        private readonly ObservableCollection<Message> _items;

        private Button _sendButton;
        private Entry _textToSendEntry;
        private ListView _messagesListView;

        public MessagesView(IObservable<Message> messagesObservable, bool showSendArea = true) 
        {
            _showSendArea = showSendArea;
            _items = new ObservableCollection<Message>();

            InitView();

            _sendButton.Clicked += async (sender, args) => await SendClicked();
            _textToSendEntry.Completed += async (sender, args) => await SendClicked();

            messagesObservable
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(m =>  _items.Add(m));
        }

        private void InitView()
        {
            var cellTemplate = new DataTemplate(typeof (TextCell));
            cellTemplate.SetBinding(TextCell.TextProperty, "Text");
            cellTemplate.SetBinding(TextCell.DetailProperty, "DetailText");

            _messagesListView = new ListView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                ItemsSource = _items,
                ItemTemplate = cellTemplate
            };

            _textToSendEntry = new Entry
            {
                Placeholder = "Enter text to send",
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            _sendButton = new Button
            {
                Text = "Send"
            };
            
            var sendArea = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { _textToSendEntry, _sendButton }
            };

            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    _messagesListView,
                    (_showSendArea ? sendArea : new StackLayout())
                }
            };

        }

        private async Task SendClicked()
        {
            var toSend = _textToSendEntry.Text;
            if (String.IsNullOrEmpty(toSend) || SendData == null) return;

            var msg = await SendData(toSend);

            if (msg != null)
                _items.Add(msg);

            _textToSendEntry.Text = "";
        }

    }
}