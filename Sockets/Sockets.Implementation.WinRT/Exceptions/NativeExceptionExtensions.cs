using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using PclSocketException = Sockets.Plugin.Abstractions.SocketException;

namespace Sockets.Plugin
{
    public static class NativeExceptionExtensions
    {
        public static Task WrapNativeSocketExceptions(this Task task)
        {
            return task.ContinueWith(
                t =>
                {
                    var ex = t.Exception.InnerException;
                    var hResult = ex.HResult;
                    var socketError = SocketError.GetStatus(hResult);
                    
                    throw (socketError == SocketErrorStatus.Unknown)
                        ? ex
                        : new PclSocketException(ex);
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public static Task<T> WrapNativeSocketExceptions<T>(this Task<T> task)
        {
            return task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        var ex = t.Exception.InnerException;
                        var hResult = ex.HResult;
                        var socketError = SocketError.GetStatus(hResult);

                        throw (socketError == SocketErrorStatus.Unknown)
                            ? ex
                            : new PclSocketException(ex);
                    }

                    return t.Result;
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}