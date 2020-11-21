// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace MicroElements.Processing.Common
{
    /// <summary>
    /// Async related extensions.
    /// </summary>
    internal static class TaskExtensions
    {
        /// <summary>
        /// This impl is better than Task.WhenAll because it avoids meaningful exception being swallowed when awaiting on error.
        /// </summary>
        public static Task AwaitableWhenAll(this Task[] tasks)
        {
            var whenAll = Task.WhenAll(tasks);
            var tcs = new TaskCompletionSource<string>();

            whenAll.ContinueWith(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetException(t.UnwrapException() !);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        tcs.SetResult(string.Empty);
                    }
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Unwraps the <see cref="Task"/>'s <see cref="AggregateException"/> if it has only on child.
        /// </summary>
        public static Exception? UnwrapException(this Task finished)
        {
            return finished.Exception?.InnerExceptions.Count == 1
                ? finished.Exception.InnerException
                : finished.Exception;
        }

        public static Exception UnwrapWithPriority(this AggregateException aggregateException)
        {
            return aggregateException.Flatten().InnerExceptions.FirstOrDefault() ?? aggregateException;
        }
    }
}
