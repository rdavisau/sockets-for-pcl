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
            var port = PortGranter.GrantPort();
            var listener = new TcpSocketListener();
            await listener.StartListeningAsync(port);

            var sut = new TcpSocketClient();
            await sut.ConnectAsync("127.0.0.1", port);

            await listener.StopListeningAsync();

            // getting here means nothing went boom
            Assert.True(true);
        }

        [Fact]
        public Task TcpSocketClient_ShouldThrowPCLException_InPlaceOfNativeSocketException()
        {
            var sut = new TcpSocketClient();
            var unreachableHostName = ":/totallynotvalid@#$";

            return Assert.ThrowsAsync<SocketException>(() => sut.ConnectAsync(unreachableHostName, 8000));
        }

        [Fact]
        public Task TcpSocketClient_ShouldThrowNormalException_WhenNotPlatformSpecific()
        {
            var sut = new TcpSocketClient();
            var tooHighForAPort = Int32.MaxValue;

            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.ConnectAsync("127.0.0.1", tooHighForAPort));
        }

        [Fact]
        public async Task TcpSocketClient_ShouldSendReceiveData()
        {
            var bytesToSend = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5};
            var len = bytesToSend.Length;
            var port = PortGranter.GrantPort();

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
            await client.ConnectAsync("127.0.0.1", port);
            await client.WriteStream.WriteAsync(bytesToSend, 0, len);

            var ok = await tcs.Task;

            await listener.StopListeningAsync();

            Assert.True(ok);
        }

        [Theory]
        [InlineData(-1)] // no buffered stream
        [InlineData(0)]
        [InlineData(1000)]
        [InlineData(100000)]
        public async Task TcpSocketClient_ShouldSendReceiveDataSimultaneously(int bufferSize)
        {
            var port = PortGranter.GrantPort();

            TcpSocketListener listener = null;
            TcpSocketClient socket1 = null;

            // get both ends of a connected socket, with or without buffer
            if (bufferSize != -1)
            {
                listener = new TcpSocketListener(bufferSize);
                socket1 = new TcpSocketClient(bufferSize);
            }
            else
            {
                listener = new TcpSocketListener();
                socket1 = new TcpSocketClient();
            }

            var tcs = new TaskCompletionSource<ITcpSocketClient>();
            
            await listener.StartListeningAsync(port);
            listener.ConnectionReceived += (sender, args) => tcs.SetResult(args.SocketClient);

            await socket1.ConnectAsync("127.0.0.1", port);

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
            Func<ITcpSocketClient, List<byte>, List<byte>, CancellationToken, Task> sendAndReceive =
                (socket, sent, recvd, token) =>
                {
                    var r = new Random(socket.GetHashCode());
                    var send = Task.Run(async () =>
                    {
                        var buf = new byte[1000];
                        while (!token.IsCancellationRequested)
                        {
                            r.NextBytes(buf);
                            sent.AddRange(buf);
                            await socket.WriteStream.WriteAsync(buf, 0, buf.Length, token);
                            await socket.WriteStream.FlushAsync();
                        }
                    });

                    var recv = Task.Run(async () =>
                    {
                        var buf = new byte[1000];
                        while (!token.IsCancellationRequested)
                        {
                            await socket.ReadStream.ReadAsync(buf, 0, buf.Length, token);
                            recvd.AddRange(buf);
                        }
                    });

                    return Task.WhenAll(send, recv);
                };

            // let the sockets run for 2.5 seconds
            var cts = new CancellationTokenSource();
            var timer = Task.Run(() => Task.Delay(TimeSpan.FromSeconds(2.5)).ContinueWith(t => cts.Cancel()));

            var socketRunners =
                Task.WhenAll(
                    Task.Run(() => sendAndReceive(socket1, sentToSocket2, recvdBySocket1, cts.Token)),
                    Task.Run(() => sendAndReceive(socket2, sentToSocket1, recvdBySocket2, cts.Token))
                    );

            await timer;
            await socketRunners;

            Debug.WriteLine("Sent to S1:{0}, Recvd by S1:{1}", sentToSocket1.Count, recvdBySocket1.Count);
            Debug.WriteLine("Sent to S2:{0}, Recvd by S2:{1}", sentToSocket2.Count, recvdBySocket2.Count);

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

        [Theory]
        [InlineData(-1)] // no buffered stream
        [InlineData(1000)] // yes buffered stream
        public async Task TcpSocketClient_ShouldBeAbleToDisconnectThenReconnect(int bufferSize)
        {
            TcpSocketClient sut = null;

            var port = PortGranter.GrantPort();
            var listener = new TcpSocketListener();
            await listener.StartListeningAsync(port);

            if (bufferSize != -1)
                sut = new TcpSocketClient(bufferSize);
            else
                sut = new TcpSocketClient();

            await sut.ConnectAsync("127.0.0.1", port);
            await sut.DisconnectAsync();

            await sut.ConnectAsync("127.0.0.1", port); 
            await sut.DisconnectAsync();

            await listener.StopListeningAsync();
        }

    }
}
