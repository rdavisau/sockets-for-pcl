using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sockets.Plugin.Abstractions
{
    public static class TcpSocketClientExtensions
    {
        #region TwoWayStream
        private sealed class TwoWayStream : Stream
        {
            private readonly Stream _readStream;
            private readonly Stream _writeStream;

            public TwoWayStream(Stream readStream, Stream writeStream)
            {
                this._readStream = readStream;
                this._writeStream = writeStream;
            }

            public override void Flush()
            {
                this._writeStream.Flush();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this._readStream != null)
                        this._readStream.Dispose();

                    if (this._writeStream != null)
                        this._writeStream.Dispose();

                }

                base.Dispose(disposing);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return this._readStream.Read(buffer, offset, count);
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return this._readStream.ReadAsync(buffer, offset, count, cancellationToken);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                this._writeStream.Write(buffer, offset, count);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return this._writeStream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }
            
            public override long Length
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotSupportedException();
                }

                set
                {
                    throw new NotSupportedException();
                }
            }
        
            public override bool CanRead
            {
                get
                {
                    return this._readStream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return this._writeStream.CanWrite;
                }
            }
        }
        #endregion

        public static Stream GetStream(this ITcpSocketClient client)
        {
            Contract.Requires(client != null);

            return new TwoWayStream(client.ReadStream, client.WriteStream);
        }
    }
}
