using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;
using Xamarin.Forms;

namespace SocketsSample.XF
{
    public class TcpSocketListenerPage : ContentPage
    {
        readonly List<ITcpSocketClient> _clients = new List<ITcpSocketClient>();
        private readonly TcpSocketListener _listener;
        
        private CancellationTokenSource _canceller;
        
        private readonly Subject<Message> _messagesSub;
        private readonly IObservable<Message> _messagesObs;

        public TcpSocketListenerPage()
        {
            _listener = new TcpSocketListener();

            _messagesSub = new Subject<Message>();
            _messagesObs = _messagesSub.AsObservable();

            _listener.ConnectionReceived += (sender, args) =>
            {
                var client = args.SocketClient;
                Device.BeginInvokeOnMainThread(() => _clients.Add(client));

                Task.Factory.StartNew(() =>
                {
                    foreach (var msg in client.ReadStrings(_canceller.Token))
                    {
                        _messagesSub.OnNext(msg);
                    }
                    
                    Device.BeginInvokeOnMainThread(()=> _clients.Remove(client));
                }, TaskCreationOptions.LongRunning);

            };

            InitView();
        }

        private void InitView()
        {
            Content = new StackLayout()
            {
				Padding = new Thickness(0, Device.OnPlatform(20,0,0), 0, 0),
                Children =
                {
                    new ListenerBindView(11000, this)
                    {
                        StartListeningTapped = async i =>
                        {
                            Debug.WriteLine("Going to listen on {0}", i);
                            await _listener.StartListeningAsync(i, Global.DefaultCommsInterface);
                            _canceller = new CancellationTokenSource();
                            return true;
                        },
                        StopListeningTapped = async () =>
                        {
                            Debug.WriteLine("Stopping Listening");
                            await _listener.StopListeningAsync();
                            _canceller.Cancel();
                        }
                    },
                    new MessagesView(_messagesObs, true)
                    {
                        SendData = async s =>
                        {
                            var sendTasks = _clients.Select(c => SocketExtensions.WriteStringAsync(c, s)).ToList();
                            await Task.WhenAll(sendTasks);

                            return new Message
                            {
                                Text = s,
                                DetailText =
                                    String.Format("Sent to {0} clients at {1}", sendTasks.Count,
                                        DateTime.Now.ToString("HH:mm:ss"))
                            };
                        }
                    }
                }
            };
        }
    }
}