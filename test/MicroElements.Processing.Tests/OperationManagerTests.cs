using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Processing.TaskManager;
using Microsoft.Extensions.Caching.Memory;
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
            var sessionManager = new SessionManager<SessionState, TaskState>(
                new SessionManagerConfiguration(),
                LoggerFactory, 
                new MemoryCache(new MemoryCacheOptions()));

            var operationManager = new OperationManager<SessionState, TaskState>(
                new OperationId("test"),
                new SessionState(),
                sessionManager,
                LoggerFactory);

            Enumerable
                .Range(1, 20)
                .Select(i => operationManager.CreateOperation(new OperationId(i.ToString()), new TaskState {Number = i}))
                .ToArray();

            await operationManager.StartAll(new ExecutionOptions<SessionState, TaskState>()
            {
                Executor = new MultiplyNumber(), 
                MaxConcurrencyLevel = 4,
                OnOperationFinished = operation1 =>
                {
                    var managerSession = operationManager.Session;
                    SessionMetrics sessionMetrics = managerSession.GetMetrics();
                    int sessionMetricsProgressInPercents = sessionMetrics.ProgressInPercents;
                }

            });

            await operationManager.SessionCompletion;

            var session = operationManager.Session;
            session.Status.Should().Be(OperationStatus.Finished);

            var operation = operationManager.GetOperation(new OperationId("2"));
            operation.State.Result.Should().Be(4);

            var metrics = session.GetMetrics();
            metrics.Should().NotBeNull();
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

    public class MultiplyNumber : ITaskExecutor<SessionState, TaskState>
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
