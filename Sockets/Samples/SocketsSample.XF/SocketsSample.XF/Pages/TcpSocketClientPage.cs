using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin;
using Xamarin.Forms;

namespace SocketsSample.XF
{
    public class TcpSocketClientPage : ContentPage
    {
        private CancellationTokenSource _canceller;
        private readonly TcpSocketClient _client;
        private readonly Subject<Message> _messagesSub;
        private readonly IObservable<Message> _messagesObs;

        public TcpSocketClientPage()
        {
            _client = new TcpSocketClient();
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
                    new ClientConnectView("127.0.0.1", 11000, this)
                    {
                        ConnectTapped = async (s, i) =>
                        {
                            await _client.ConnectAsync(s, i);
                            _canceller = new CancellationTokenSource();

                            Task.Factory.StartNew(() =>
                            {
                                foreach (var msg in _client.ReadStrings(_canceller.Token))
                                {
                                    _messagesSub.OnNext(msg);
                                }
                            }, TaskCreationOptions.LongRunning);

                            return true;
                        },
                        DisconnectTapped = async () =>
                        {
                            var bytes = Encoding.UTF8.GetBytes("<EOF>");
                            await _client.WriteStream.WriteAsync(bytes, 0, bytes.Length);
                            await _client.WriteStream.FlushAsync();

                            _canceller.Cancel();
                            await _client.DisconnectAsync();
                        }
                    },
                    new MessagesView(_messagesObs, true)
                    {
                        SendData = async s =>
                        {
                            await _client.WriteStringAsync(s);

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