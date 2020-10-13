// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroElements.Processing.Common
{
    /// <summary>
    /// AwaitableLock based on <see cref="SemaphoreSlim"/>.
    /// Implements <see cref="IDisposable"/> to use in using block.
    /// It waits for semaphore and releases it on dispose.
    /// </summary>
    internal static class AwaitableLock
    {
        internal readonly struct LockLease : IDisposable
        {
            private readonly SemaphoreSlim _lock;

            internal LockLease(SemaphoreSlim @lock) => _lock = @lock;

            /// <inheritdoc/>
            public void Dispose() => _lock.Release();
        }

        /// <summary>
        /// Waits async for <paramref name="semaphoreSlim"/> and returns <see cref="IDisposable"/> that will release lock on dispose.
        /// </summary>
        /// <param name="semaphoreSlim">SemaphoreSlim.</param>
        /// <returns><see cref="IDisposable"/> that will release lock on dispose.</returns>
        public static async ValueTask<LockLease> WaitAsyncAndGetLockReleaser(this SemaphoreSlim semaphoreSlim)
        {
            await semaphoreSlim.WaitAsync();
            return new LockLease(semaphoreSlim);
        }

        /// <summary>
        /// Waits for <paramref name="semaphoreSlim"/> and returns <see cref="IDisposable"/> that will release lock on dispose.
        /// </summary>
        /// <param name="semaphoreSlim">SemaphoreSlim.</param>
        /// <returns><see cref="IDisposable"/> that will release lock on dispose.</returns>
        public static LockLease WaitAndGetLockReleaser(this SemaphoreSlim semaphoreSlim)
        {
            semaphoreSlim.Wait();
            return new LockLease(semaphoreSlim);
        }
    }
}
