using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PclSocketException = Sockets.Plugin.Abstractions.SocketException;
using PlatformSocketException = System.Net.Sockets.SocketException;

namespace Sockets.Plugin.Abstractions
{
    public static class NativeExceptionExtensions
    {
        internal static readonly HashSet<Type> NativeSocketExceptions = new HashSet<Type> { typeof(PlatformSocketException) };

        public static Task WrapNativeSocketExceptions(this Task task)
        {
            return task.ContinueWith(
                t =>
                {
                    var ex = t.Exception.InnerException;

                    throw (NativeExceptionExtensions.NativeSocketExceptions.Contains(ex.GetType())) 
                        ? new PclSocketException(ex)
                        : ex;
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

                        throw (NativeExceptionExtensions.NativeSocketExceptions.Contains(ex.GetType()))
                            ? new PclSocketException(ex)
                            : ex;
                    }

                    return t.Result;
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
