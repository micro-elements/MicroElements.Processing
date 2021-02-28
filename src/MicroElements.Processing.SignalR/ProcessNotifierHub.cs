// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace MicroElements.Processing.SignalR
{
    /// <summary>
    /// SignalR ProcessNotifier Hub.
    /// </summary>
    public class ProcessNotifierHub : Hub<IProcessNotifier>
    {
        public async Task OperationUpdate(OperationUpdateMessage message)
        {
            await Clients.All.OperationUpdate(message);
        }
    }
}
