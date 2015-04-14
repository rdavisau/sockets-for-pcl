using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Sockets.Plugin;
using Xamarin.Forms;

namespace SocketsSample.XF
{
    public class UdpSocketMulticastClientPage : ContentPage
    {
        private UdpSocketMulticastClient _client;
        private Subject<Message> _messagesSub;
        private IObservable<Message> _messagesObs;
        private CancellationTokenSource _canceller;

        public UdpSocketMulticastClientPage()
        {
            _client = new UdpSocketMulticastClient();
            _messagesSub = new Subject<Message>();
            _messagesObs = _messagesSub.AsObservable();

            InitView();
        }

        private void InitView()
        {
            Content = new StackLayout()
            {
                Children =
                {
                    new ClientConnectView("239.192.0.1", 11811, this)
                    {
                        ConnectTapped = async (s, i) =>
                        {
                            await _client.JoinMulticastGroupAsync(s, i);
                            _canceller = new CancellationTokenSource();

                            _client.MessageReceived += (sender, args) =>
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

                            return true;
                        },
                        DisconnectTapped = async () =>
                        {
                            var bytes = "<EOF>".ToUTF8Bytes();
                            await _client.SendMulticastAsync(bytes);

                            _canceller.Cancel();
                            await _client.DisconnectAsync();
                        }
                    },
                    new MessagesView(_messagesObs, true)
                    {
                        SendData = async s =>
                        {
                            var bytes = s.ToUTF8Bytes();
                            await _client.SendMulticastAsync(bytes);

                            return new Message
                            {
                                Text = s,
                                DetailText = String.Format("Sent at {0}", DateTime.Now.ToString("HH:mm:ss"))
                            };
                        }
                    }
                }
            };
        }
    }
}