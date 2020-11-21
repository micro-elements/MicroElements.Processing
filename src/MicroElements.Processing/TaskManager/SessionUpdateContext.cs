// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using MicroElements.Metadata;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Update context for <see cref="ISession{TSessionState}"/>.
    /// </summary>
    /// <typeparam name="TSessionState">Session state.</typeparam>
    public class SessionUpdateContext<TSessionState>
    {
        /// <summary>
        /// Gets current session.
        /// </summary>
        public ISession<TSessionState> Session { get; }

        /// <summary>
        /// Gets or sets new state for session.
        /// </summary>
        [MaybeNull]
        [AllowNull]
        public TSessionState NewState { get; set; } = default!;

        /// <summary>
        /// Gets or sets new metadata for session.
        /// </summary>
        public IPropertyContainer? NewMetadata { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionUpdateContext{TSessionState}"/> class.
        /// </summary>
        /// <param name="session">Current session state.</param>
        public SessionUpdateContext(ISession<TSessionState> session)
        {
            Session = session;
        }
    }
}
