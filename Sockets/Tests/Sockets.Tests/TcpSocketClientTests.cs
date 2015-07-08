using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Sockets.Tests
{
    public class TcpSocketClientTests
    {
        [Fact]
        public void TcpSocketClient_Constructor_ShouldSucceed()
        {
            // just checking bait and switch is set up
            var socket = new TcpSocketClient();
            Assert.True(true);
        }

        [Fact]
        public async Task TcpSocketClient_ShouldBeAbleToConnect()
        {
            var port = 51234;
            var listener = new TcpSocketListener();
            await listener.StartListeningAsync(port);

            var sut = new TcpSocketClient();
            await sut.ConnectAsync("localhost", port);

            await listener.StopListeningAsync();

            // getting here means nothing went boom
            Assert.True(true);
        }

        [Fact]
        public Task TcpSocketClient_ShouldThrowPCLException_InPlaceOfNativeSocketException()
        {
            var sut = new TcpSocketClient();
            var unreachableHostName = ":/totallynotvalid@#$";

            return Assert.ThrowsAsync<Exception>(() => sut.ConnectAsync(unreachableHostName, 8000));
        }

        [Fact]
        public Task TcpSocketClient_ShouldThrowNormalException_WhenNotPlatformSpecific()
        {
            var sut = new TcpSocketClient();
            var tooHighForAPort = Int32.MaxValue;

            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.ConnectAsync("localhost", tooHighForAPort));
        }

        [Fact]
        public async Task TcpSocketClient_ShouldSendReceiveData()
        {
            var bytesToSend = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5};
            var len = bytesToSend.Length;
            var port = 51234;

            var listener = new TcpSocketListener();
            await listener.StartListeningAsync(port);

            var tcs = new TaskCompletionSource<bool>();
            listener.ConnectionReceived += async (sender, args) =>
            {
                var bytesReceived = new byte[len];
                await args.SocketClient.ReadStream.ReadAsync(bytesReceived, 0, len);

                var allSame =
                    Enumerable
                        .Zip(bytesToSend,
                            bytesReceived,
                            (s, r) => s == r)
                        .All(b => b);

                tcs.SetResult(allSame);
            };

            var client = new TcpSocketClient();
            await client.ConnectAsync("localhost", port);
            await client.WriteStream.WriteAsync(bytesToSend, 0, len);

            var ok = await tcs.Task;

            await listener.StopListeningAsync();

            Assert.True(ok);
        }

        [Fact]
        public async Task TcpSocketClient_ShouldSendReceiveDataSimultaneously()
        {
            var port = 51234;

            // get both ends of a connected socket
            var tcs = new TaskCompletionSource<ITcpSocketClient>();
            var listener = new TcpSocketListener();
            await listener.StartListeningAsync(port);
            listener.ConnectionReceived += (sender, args) => tcs.SetResult(args.SocketClient);

            var socket1 = new TcpSocketClient();
            await socket1.ConnectAsync("localhost", port);

            var socket2 = await tcs.Task;
            await listener.StopListeningAsync();

            // socket1 is the socket from the client end
            // socket2 is the socket from the server end

            // for five seconds, send and receive the data 
            var sentToSocket1 = new List<byte>();
            var sentToSocket2 = new List<byte>();
            var recvdBySocket1 = new List<byte>();
            var recvdBySocket2 = new List<byte>();

            // send random data and keep track of it
            // also keep track of what is received
            Action<ITcpSocketClient, List<byte>, List<byte>, CancellationToken> sendAndReceive =
                (socket, sent, recvd, token) =>
                {
                    var r = new Random(socket.GetHashCode());
                    Task.Run(async () =>
                    {
                        var buf = new byte[1];
                        while (true)
                        {
                            r.NextBytes(buf);
                            sent.AddRange(buf);
                            await socket.WriteStream.WriteAsync(buf,0,1, token);
                            await socket.WriteStream.FlushAsync();
                        }
                    }, token);

                    Task.Run(async () =>
                    {
                        var buf = new byte[1];
                        while (true)
                        {
                            await socket.ReadStream.ReadAsync(buf,0,1, token);
                            recvd.AddRange(buf);
                        }
                    }, token);
                };

            // let the sockets run for five seconds
            var cts = new CancellationTokenSource();
            var timer = Task.Run(() => Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(t => cts.Cancel()));

            Task.Run(() => sendAndReceive(socket1, sentToSocket2, recvdBySocket1, cts.Token));
            Task.Run(() => sendAndReceive(socket2, sentToSocket1, recvdBySocket2, cts.Token));

            await timer;

            // zip will join up to the lowest index of both lists (must be recvd)
            // it's ok if recvd is shorter than sent because we cancel abruptly,
            // but we want to be sure that everything that was received matches 
            // everything that sent, and that we did both send and receive.

            var socket1OK =
                Enumerable.Zip(sentToSocket1,
                            recvdBySocket1,
                            (s, r) => s == r)
                        .All(b => b);

            var socket2OK =
                Enumerable.Zip(sentToSocket1,
                            recvdBySocket1,
                            (s, r) => s == r)
                        .All(b => b);

            Assert.True(socket1OK, "Socket1 received data did not match what was sent to it");
            Assert.True(socket2OK, "Socket2 received data did not match what was sent to it");

            // eww, but does the job we need
            Assert.True(recvdBySocket1.Count > 5000, String.Format("Socket 1 received data count was less than 5000 : {0}", recvdBySocket1.Count));
            Assert.True(recvdBySocket2.Count > 5000, String.Format("Socket 2 received data count was less than 5000 : {0}", recvdBySocket2.Count));

            await socket1.DisconnectAsync();
            await socket2.DisconnectAsync();
        }

        [Fact]
        public async Task TcpSocketClient_ShouldBeAbleToDisconnectThenReconnect()
        {
            var port = 51234;
            var listener = new TcpSocketListener();
            await listener.StartListeningAsync(port);

            var sut = new TcpSocketClient();

            await sut.ConnectAsync("localhost", port);
            await sut.DisconnectAsync();

            await sut.ConnectAsync("localhost", port);
            await sut.DisconnectAsync();

            await listener.StopListeningAsync();
        }

    }


}
