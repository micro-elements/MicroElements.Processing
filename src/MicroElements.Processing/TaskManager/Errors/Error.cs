// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Provides generic error information.
    /// </summary>
    public class Error<TErrorCode> : IError<TErrorCode>
    {
        /// <summary>
        /// Gets known error code.
        /// </summary>
        public TErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets error message.
        /// </summary>
        public Message Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">Known error code.</param>
        /// <param name="message">Error message.</param>
        public Error(TErrorCode errorCode, Message message)
        {
            ErrorCode = errorCode;
            Message = message;
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(ErrorCode)}: {ErrorCode}, {nameof(Message)}: {Message.FormattedMessage}";
    }

    /// <summary>
    /// Static error helpers.
    /// </summary>
    public static class Error
    {
        private static class ErrorCache<TErrorCode>
        {
            public static readonly IError<TErrorCode> Empty = new Error<TErrorCode>(default, new Message("Error"));
        }

        /// <summary>
        /// Provides empty error.
        /// </summary>
        /// <typeparam name="TErrorCode">Error code.</typeparam>
        /// <returns>Singleton instance of empty error.</returns>
        public static IError<TErrorCode> Empty<TErrorCode>() => ErrorCache<TErrorCode>.Empty;

        /// <summary>
        /// Creates error.
        /// </summary>
        /// <typeparam name="TErrorCode">Error code type.</typeparam>
        /// <param name="errorCode">Error code.</param>
        /// <param name="messageTemplate">Message template. Can be in form of MessageTemplates.org.</param>
        /// <param name="args">Args for <paramref name="messageTemplate"/>.</param>
        /// <returns><see cref="IError"/> instance.</returns>
        public static IError<TErrorCode> CreateError<TErrorCode>(TErrorCode errorCode, string messageTemplate, params object[] args)
        {
            Message message = new Message(
                eventName: ErrorCode.SessionDoesNotExists.ToString(),
                severity: MessageSeverity.Error,
                originalMessage: messageTemplate).WithArgs(args ?? Array.Empty<object>());
            return new Error<TErrorCode>(errorCode, message);
        }

        /// <summary>
        /// Creates error from exception.
        /// </summary>
        /// <typeparam name="TErrorCode">Error code type.</typeparam>
        /// <param name="e">Exception.</param>
        /// <returns><see cref="IError"/> instance.</returns>
        public static IError<TErrorCode> CreateError<TErrorCode>(Exception e)
        {
            if (e is ExceptionWithError<TErrorCode> knownException)
            {
                return knownException.Error;
            }

            return CreateError<TErrorCode>(default, e.Message);
        }

        public static IError<ErrorCode>? Try(Action action)
        {
            try
            {
                action();
                return default;
            }
            catch (Exception e)
            {
                return Error.CreateError<ErrorCode>(e);
            }
        }

        public static IError<ErrorCode>? Try(Func<IError<ErrorCode>?> action)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                return Error.CreateError<ErrorCode>(e);
            }
        }
    }
}
