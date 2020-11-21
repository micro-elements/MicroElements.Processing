# 1.0.0-rc.1
- MicroElements dependencies updated to latest
- Nullable annotations added
- All code documented

# 0.12.0
- MicroElements.Functional updated to 1.5.0
- MicroElements.Metadata updated to 4.6.0
- MicroElements.Data updated to 1.1.0

# 0.11.0
- MicroElements.Metadata updated to 4.3.0

# 0.10.0
- New: OperationManager Tracing added (supports OpenTelemetry with 'MicroElements.Processing' source name).
- Tracer added to OperationExecutionContext to have ability trace children operation activities
- OperationStatusWithError added
- MicroElements libs updated
- fixed nulls in SessionMetricsMeta.PropertySet

# 0.9.0
- MicroElements libs updated
- SessionMetrics now constructed by metadata and can be extended.

# 0.8.0
- Error processing unification.
- Added System.Diagnostics.DiagnosticSource to provide initial OpenTelemetry support
- Added SessionUpdateContext.NewMetadata to update metadata for session
- SessionManager adds its metadata to added OperatioManagers
- SessionManager provides new metadata GlobalConcurrencyLevel
- OperatioManager adds metadata OperationMeta.GlobalWaitDuration to operation after global lock accured
- OperatioManager can use IOperationExecutorExtended instead IOperationExecutor. See IExecutionOptions.
- SessionMetrics now implements IMetadataProvider with metadata properties from SessionMetricsMeta
- Added metrics to SessionMetrics: GlobalConcurrencyLevel, ProcessorCount, AvgMillisecondsPerOperation, TotalWaitingTime, AvgProcessingTimePerOperation, AvgWaitingTimePerOperation

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
