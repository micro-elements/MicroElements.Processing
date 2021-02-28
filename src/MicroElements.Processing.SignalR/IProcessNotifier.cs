// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using MicroElements.Metadata;
using MicroElements.Processing.TaskManager;

namespace MicroElements.Processing.SignalR
{
    /// <summary>
    /// SignalR process notifier.
    /// </summary>
    public interface IProcessNotifier
    {
        /// <summary>
        /// Notify subscribers about operation state changed.
        /// </summary>
        /// <param name="message">Notification message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task OperationUpdate(OperationUpdateMessage message);
    }

    public class OperationUpdateMessage: IManualMetadataProvider
    {
        public OperationUpdateMessage(IPropertyContainer? metadata = null)
        {
            Metadata = metadata ?? new MutablePropertyContainer();
        }

        public string SessionId { get; set; }
        public string SessionStatus { get; set; }
        public string OperationId { get; set; }
        public string OperationStatus { get; set; }

        /// <inheritdoc />
        public IPropertyContainer Metadata { get; private set; }
    }

    public class aa<TSessionState, TOperationState>
    {
        void OnOperationFinished(ISession<TSessionState> session, IOperation<TOperationState> operation)
        {
            IPropertyContainer mergedMetadata = PropertyContainer.Merge(
                PropertyAddMode.Set,
                session.Metadata,
                session.GetMetrics().Metadata,
                operation.Metadata);

            OperationUpdateMessage message = new OperationUpdateMessage(mergedMetadata)
            {
                SessionId = session.Id.ToString(),
                SessionStatus = session.Status.ToString(),
                OperationId = operation.Id.ToString(),
                OperationStatus = operation.Status.ToString(),
            };
        }
    }
}
