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

        // different things cause different exceptions on .NET vs WinRT
        // so we swap the method that's called.
        // mfw

        [Fact]
        public Task TcpSocketClient_ShouldThrowPCLException_InPlaceOfNativeSocketException()
        {
#if WINDOWS_UWP
            var t = OnlyThrowsSocketExceptionOnWinRT();
#else
            var t = OnlyThrowsSocketExceptionOnNet();
#endif
            return Assert.ThrowsAnyAsync<SocketException>(() => t);
        }

        [Fact]
        public Task TcpSocketClient_ShouldThrowNormalException_WhenNotPlatformSpecific()
        {
#if WINDOWS_UWP
            return Assert.ThrowsAsync<ArgumentException>(OnlyThrowsSocketExceptionOnNet);
#else
            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(OnlyThrowsSocketExceptionOnWinRT);
#endif

        }

        private Task OnlyThrowsSocketExceptionOnNet()
        {
            var sut = new TcpSocketClient();
            var unreachableHostName = ":/totallynotvalid@#$";

            return sut.ConnectAsync(unreachableHostName, 8000);
        }

        private Task OnlyThrowsSocketExceptionOnWinRT()
        {
            var sut = new TcpSocketClient();
            var tooHighForAPort = Int32.MaxValue;

            return sut.ConnectAsync("127.0.0.1", tooHighForAPort);
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
                            await socket.WriteStream.FlushAsync(token);
                        }
                    });

                    var recv = Task.Run(async () =>
                    {
                        var buf = new byte[1000];
                        while (!token.IsCancellationRequested)
                        {
                            var len = await socket.ReadStream.ReadAsync(buf, 0, buf.Length, token);
                            recvd.AddRange(buf.Take(len));
                        }
                    });

                    var innerTcs = new TaskCompletionSource<bool>();
                    token.Register(() => innerTcs.SetResult(true));

                    return innerTcs.Task;
                };

            // let the sockets run for 2.5 seconds
            var socketRunners =
                Task.WhenAll(
                    Task.Run(() => sendAndReceive(socket1, sentToSocket2, recvdBySocket1, new CancellationTokenSource(TimeSpan.FromSeconds(2.5)).Token).ContinueWith(t=> Debug.WriteLine($"Socket 1 task completed: {t}"))),
                    Task.Run(() => sendAndReceive(socket2, sentToSocket1, recvdBySocket2, new CancellationTokenSource(TimeSpan.FromSeconds(2.5)).Token).ContinueWith(t => Debug.WriteLine($"Socket 2 task completed: {t}")))
                    );

            await socketRunners;

            Debug.WriteLine("Sent to S1:{0}, Recvd by S1:{1}", sentToSocket1.Count, recvdBySocket1.Count);
            Debug.WriteLine("Sent to S2:{0}, Recvd by S2:{1}", sentToSocket2.Count, recvdBySocket2.Count);

            // zip will join up to the lowest index of both lists (must be recvd)
            // it's ok if recvd is shorter than sent because we cancel abruptly,
            // but we want to be sure that everything that was received matches 
            // everything that sent, and that we did both send and receive.

            var socket1Errors =
                Enumerable.Zip(recvdBySocket1, sentToSocket1,
                            (r, s) => s == r)
                        .Count(b => !b);

            var socket2Errors =
                Enumerable.Zip(recvdBySocket2, 
                                sentToSocket2,
                            (r, s) => s == r)
                        .Count(b => !b);
            
            var max = new[] {recvdBySocket1.Count, recvdBySocket2.Count, sentToSocket1.Count, sentToSocket2.Count}.Max();
            Func<List<byte>, int, string> getValueOrStars =
                (bytes, i) => bytes.Count > i ? ((int) bytes[i]).ToString().PadLeft(3, '0') : "***";

            var rows =
                Enumerable.Range(0, max - 1)
                    .Select(i =>
                    {
                        var sTo1 = getValueOrStars(sentToSocket1, i);
                        var rBy1 = getValueOrStars(recvdBySocket1, i);
                        var sTo2 = getValueOrStars(sentToSocket2, i);
                        var rBy2 = getValueOrStars(recvdBySocket2, i);

                        return new { Index = i, SentTo1 = sTo1, ReceivedBy1 = rBy1, SentTo2 = sTo2, ReceivedBy2 = rBy2};
                    })
                    .Where(r => (r.SentTo1 != r.ReceivedBy1 && r.ReceivedBy1 != "***") || (r.SentTo2 != r.ReceivedBy2 && r.ReceivedBy2 != "***"))
                    .Select(r=> $"{r.Index}: \t{r.SentTo1}\t{r.ReceivedBy1}\t{r.SentTo2}\t{r.ReceivedBy2}\t")
                    .Take(1000);
            
            Func<IEnumerable<byte>, string> show = bs => $"[ {String.Join(", ", bs.Take(5).Select(b => (int) b))} ]";

            if (socket1Errors > 0 || socket2Errors > 0)
            {
                Assert.True(socket1Errors == 0,
                    $"Socket1 received {socket1Errors} byte/s that did not match what was sent to it{Environment.NewLine}{String.Join(Environment.NewLine, rows)}");
                Assert.True(socket2Errors == 0,
                    $"Socket2 received {socket2Errors} byte/s that did not match what was sent to it{Environment.NewLine}{String.Join(Environment.NewLine, rows)}");
            }
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

        [Fact]
        public Task TcpSocketClient_Connect_ShouldCancelByCancellationToken()
        {
            var sut = new TcpSocketClient();
            
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var ct = cts.Token;

            // let's just hope no one's home :)
            return Assert.ThrowsAsync<OperationCanceledException>(()=> sut.ConnectAsync("99.99.99.99", 51234, cancellationToken: cts.Token));
        }

        [Fact]
        public void TcpSocketClient_NoDelay_CanBeSetAndRead()
        {
            var sut = new TcpSocketClient();
            Assert.False(sut.NoDelay);
            sut.NoDelay = true;
            Assert.True(sut.NoDelay);
        }

    }
}
