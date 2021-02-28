// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace MicroElements.Processing.SignalR
{
    /// <summary>
    /// <see cref="IProcessNotifier"/> that uses SignalR <see cref="ProcessNotifierHub"/> to notify clients.
    /// <para><see cref="ProcessNotifierHub"/> should be registered in AspNetCore services.</para>
    /// </summary>
    public class ProcessNotifierSignalR : IProcessNotifier
    {
        private readonly IHubContext<ProcessNotifierHub, IProcessNotifier> _hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessNotifierSignalR"/> class.
        /// </summary>
        /// <param name="hubContext"><see cref="IHubContext{THub}"/> that injected by AspNetCore DI.</param>
        public ProcessNotifierSignalR(IHubContext<ProcessNotifierHub, IProcessNotifier> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <inheritdoc />
        public Task OperationUpdate(OperationUpdateMessage message)
        {
            return _hubContext.Clients.All.OperationUpdate(message);
        }
    }
}
