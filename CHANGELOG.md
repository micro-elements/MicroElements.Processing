# 0.7.0
- Added ISessionStorage.Delete method.
- ISessionManager: fixed Delete.
- Added: GetOperationManagerOrThrow and GetSessionOrThrow extensions

# 0.6.0
- IOperationManager: Added UpdateSession and new UpdateOperation
- ISession: Added ExecutionOptions
- SessionMetrics: Added MaxConcurrencyLevel and SpeedupRatio

# 0.5.0
- ISession<TSessionState> has session state and messages but does not evaluates operation list
- IExecutionOptions.OnOperationFinished now accepts ISession<TSessionState>
- OperationManager now has property Session of type ISession<TSessionState> to access get session without operations and property SessionWithOperations to get full session with operations.
- Added ISessionStorage to abstract session storage
- Minor API changes

# 0.4.0
- SessionMetrics: Added Duration and Estimation
- SessionMetrics: ProgressInPercents fixed

# 0.3.0
- SessionMetrics extended: AvgMillisecondsPerOperation, OperationsPerMinute, OperationsPerSecond
- Added IExecutionOptions.OnOperationFinished callback
- Minor API changes

# 0.2.0
- Added SessionMetrics
- Added Services to SessionManager that can be used in OperationManager

# 0.1.0
- Added Pipeline for pipeline crestion
- Added SessionManager and OperationManager for LongTerm operations.

Full release notes can be found at: https://github.com/micro-elements/MicroElements.Processing.git/blob/master/CHANGELOG.md
