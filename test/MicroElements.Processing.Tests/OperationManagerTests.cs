using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Processing.TaskManager;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace MicroElements.Processing.Tests
{
    public class OperationManagerTests : UnitTestWithOutput
    {
        /// <inheritdoc />
        public OperationManagerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task ExecuteOperations()
        {
            var configuration = new SessionManagerConfiguration();
            var sessionStorage = new MemoryCacheStorage<SessionState, TaskState>(configuration, new MemoryCache(new MemoryCacheOptions()));
            var sessionManager = new SessionManager<SessionState, TaskState>(configuration, LoggerFactory, sessionStorage);

            var operationManager = new OperationManager<SessionState, TaskState>(
                new OperationId("test"),
                new SessionState(),
                sessionManager);

            Enumerable
                .Range(1, 20)
                .Select(i => operationManager.CreateOperation(new OperationId(i.ToString()), new TaskState {Number = i}))
                .ToArray();

            await operationManager.StartAll(new ExecutionOptions<SessionState, TaskState>()
            {
                Executor = new MultiplyNumber(), 
                MaxConcurrencyLevel = 4,
                OnOperationFinished = OnOperationFinished,
                OnSessionFinished = OnSessionFinished
            });

            await operationManager.SessionCompletion;

            var session = operationManager.SessionWithOperations;
            session.Status.Should().Be(OperationStatus.Finished);

            var operation = operationManager.GetOperation(new OperationId("2"));
            operation.State.Result.Should().Be(4);

            var metrics = session.GetMetrics();
            metrics.Should().NotBeNull();
        }

        private void OnSessionFinished(ISession<SessionState, TaskState> obj)
        {
            Logger.LogInformation("SessionFinished");
        }

        private void OnOperationFinished(ISession<SessionState> session, IOperation<TaskState> operation)
        {
            SessionMetrics sessionMetrics = session.GetMetrics();
            Logger.LogInformation($"Progress: {sessionMetrics.ProgressInPercents}");
        }
    }

    public class SessionState
    {
        //Example session state
    }

    public class TaskState
    {
        public int Number { get; set; }

        public int Result { get; set; }
    }

    public class MultiplyNumber : IOperationExecutor<SessionState, TaskState>
    {
        /// <inheritdoc />
        public async Task<IOperation<TaskState>> ExecuteAsync(ISession<SessionState> session, IOperation<TaskState> operation, CancellationToken cancellation = default)
        {
            // Sample of mutable state
            operation.State.Result = operation.State.Number * 2;

            await Task.Delay(1000);

            // Session can be accessed
            session.Messages.Add(new Message($"Input: {operation.State.Number}, Result: {operation.State.Result}"));

            // the same operation
            return operation;
        }
    }
}
