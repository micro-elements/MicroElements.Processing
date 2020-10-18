// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Known errors.
    /// </summary>
    public static class Errors
    {
        public static IError<ErrorCode> SessionDoesNotExists(OperationId sessionId) => Error.CreateError(
            errorCode: ErrorCode.SessionDoesNotExists,
            messageTemplate: "Session does not exists. SessionId: {sessionId}.",
            args: sessionId);

        public static IError<ErrorCode> SessionIsNotStarted(OperationId sessionId) => Error.CreateError(
            errorCode: ErrorCode.SessionIsNotStarted,
            messageTemplate: "Session is not started. SessionId: {sessionId}.",
            args: sessionId);

        public static IError<ErrorCode> SessionIsAlreadyStarted(OperationId sessionId) => Error.CreateError(
            errorCode: ErrorCode.SessionIsAlreadyStarted,
            messageTemplate: "Session is already started. SessionId: {sessionId}.",
            args: sessionId);

        public static IError<ErrorCode> SessionUpdateIsProhibited(OperationId sessionId, string sessionStatus) => Error.CreateError(
            errorCode: ErrorCode.SessionUpdateIsProhibited,
            messageTemplate: "Session update is prohibited. You can change session only in NotStarted status. SessionId: {sessionId}, SessionStatus: {sessionStatus}.",
            args: new object[] { sessionId, sessionStatus });

        public static IError<ErrorCode> OperationDoesNotExists(OperationId operationId) => Error.CreateError(
            errorCode: ErrorCode.OperationDoesNotExists,
            messageTemplate: "Operation does not exists. OperationId: {operationId}.",
            args: operationId);

        public static IError<ErrorCode> OperationIdDoesNotMatch(OperationId providedOperationId, OperationId existingOperationId) => Error.CreateError(
            errorCode: ErrorCode.OperationIdDoesNotMatch,
            messageTemplate: "Provided OperationId does not match existing OperationId. ProvidedOperationId: {providedOperationId}, ExistingOperationId: {existingOperationId}.",
            args: new object[] { providedOperationId, existingOperationId });
    }
}
