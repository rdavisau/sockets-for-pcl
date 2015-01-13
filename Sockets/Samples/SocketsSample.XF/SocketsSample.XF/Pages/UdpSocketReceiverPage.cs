using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Sockets.Plugin;
using Xamarin.Forms;

namespace SocketsSample.XF
{
    public class UdpSocketReceiverPage : ContentPage
    {
        private UdpSocketReceiver _receiver;
        private Subject<Message> _messagesSub;
        private IObservable<Message> _messagesObs;

        public UdpSocketReceiverPage()
        {
            _receiver = new UdpSocketReceiver();
            _messagesSub = new Subject<Message>();
            _messagesObs = _messagesSub.AsObservable();

            _receiver.MessageReceived += (sender, args) =>
            {
                var data = args.ByteData.ToStringFromUTF8Bytes();
                var from = String.Format("{0}:{1}", args.RemoteAddress, args.RemotePort);

                var msg = new Message
                {
                    Text = data,
                    DetailText = String.Format("<Received from {1} at {0}>", DateTime.Now.ToString("HH:mm:ss"), from)
                };

                _messagesSub.OnNext(msg);
            };

            InitView();
        }

        private void InitView()
        {
            Content = new StackLayout
            {
                Children =
                {
                    new ListenerBindView(11011, this)
                    {
                        StartListeningTapped = async i =>
                        {
                            await _receiver.StartListeningAsync(i);
                            return true;
                        },
                        StopListeningTapped = async () =>
                        {
                            await _receiver.StopListeningAsync();
                        }
                    },
                    new MessagesView(_messagesObs, false)
                }
            };
        }
    }
}