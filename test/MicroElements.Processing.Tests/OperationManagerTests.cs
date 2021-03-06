using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
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
        public void added_operation_should_be_in_initial_state()
        {
            var operationManager = CreateOperationManager();

            IOperation<TaskState> operation = operationManager.CreateOperation("1", new TaskState(){Number = 1});

            operation.Id.Should().Be(new OperationId("1"));
            operation.State.Number.Should().Be(1);
            operation.Status.Should().Be(OperationStatus.NotStarted);
            operation.StartedAt.Should().Be(null);
            operation.FinishedAt.Should().Be(null);
            operation.Exception.Should().Be(null);
        }

        [Fact]
        public void added_operation_should_be_accessed_in_operation_manager()
        {
            var operationManager = CreateOperationManager();

            IOperation<TaskState> operation = operationManager.CreateOperation("1", new TaskState() { Number = 1 });

            operationManager
                .GetOperations()
                .Should().HaveCount(1)
                .And.Subject
                .Single().Should().BeSameAs(operation);

            operationManager
                .GetOperation("1")
                .Should().BeSameAs(operation);
        }

        [Fact]
        public void update_operation_should_be_successful()
        {
            var operationManager = CreateOperationManager();

            IOperation<TaskState> operation = operationManager.CreateOperation("1", new TaskState() { Number = 1 });

            operationManager.UpdateOperation("1", operation
                .With(status: OperationStatus.Finished)
                .WithState(state: new TaskState() {Number = 1, Result = 2}));

            operationManager
                .GetOperations()
                .Should().HaveCount(1);

            var updated = operationManager.GetOperation("1")!;
            updated.Id.Should().Be(new OperationId("1"));
            updated.State.Result.Should().Be(2);
            updated.Status.Should().Be(OperationStatus.Finished);
        }

        [Fact]
        public void update_operation_should_fail()
        {
            var operationManager = CreateOperationManager();
            Action action = () => operationManager.UpdateOperation("1", updatedOperation: null!);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void delete_operation_should_be_successful()
        {
            var operationManager = CreateOperationManager();

            var added = operationManager.CreateOperation("1", new TaskState() { Number = 1 });

            var deleted = operationManager.DeleteOperation("1");

            operationManager
                .GetOperations()
                .Should().BeEmpty();

            deleted.Should().BeSameAs(added);

            operationManager
                .GetOperation("1")
                .Should().BeNull();
        }

        [Fact]
        public async Task singlethreaded_simulation_should_be_successful()
        {
            IOperationManager<SessionState, TaskState> operationManager = CreateOperationManager();

            Enumerable
                .Range(1, 5)
                .Select(i => operationManager.CreateOperation(new OperationId(i.ToString()), new TaskState { Number = i }))
                .ToArray();

            await operationManager.Start(new ExecutionOptions<SessionState, TaskState>()
            {
                Executor = new MultiplyByTwo(),
                MaxConcurrencyLevel = 1,
                OnOperationFinished = OnOperationFinished,
                OnSessionFinished = OnSessionFinished
            });

            await operationManager.SessionCompletion;

            var session = operationManager.SessionWithOperations;
            session.Status.Should().Be(OperationStatus.Finished);

            var operation = operationManager.GetOperation(new OperationId("2"))!;
            operation.State.Result.Should().Be(4);

            var metrics = session.GetMetrics();
            metrics.Should().NotBeNull();
        }

        [Fact]
        public async Task multithreaded_simulation_should_be_successful()
        {
            IOperationManager<SessionState, TaskState> operationManager = CreateOperationManager();

            Enumerable
                .Range(1, 20)
                .Select(i => operationManager.CreateOperation(new OperationId(i.ToString()), new TaskState {Number = i}))
                .ToArray();

            await operationManager.Start(new ExecutionOptions<SessionState, TaskState>()
            {
                ExecutorExtended = new MultiplyByTwoExt(), 
                MaxConcurrencyLevel = 5,
                OnOperationFinished = OnOperationFinished,
                OnSessionFinished = OnSessionFinished
            });

            await operationManager.SessionCompletion;

            var session = operationManager.SessionWithOperations;
            session.Status.Should().Be(OperationStatus.Finished);

            var operation = operationManager.GetOperation(new OperationId("2"))!;
            operation.State.Result.Should().Be(4);

            var metrics = session.GetMetrics();
            metrics.Should().NotBeNull();
        }

        [Fact]
        public async Task multithreaded_simulation_should_be_cancelled()
        {
            IOperationManager<SessionState, TaskState> operationManager = CreateOperationManager();

            Enumerable
                .Range(1, 20)
                .Select(i => operationManager.CreateOperation(new OperationId(i.ToString()), new TaskState { Number = i }))
                .ToArray();

            await operationManager.Start(new ExecutionOptions<SessionState, TaskState>()
            {
                Executor = new MultiplyByTwo(),
                MaxConcurrencyLevel = 4,
                OnOperationFinished = OnOperationFinished,
                OnSessionFinished = OnSessionFinished
            });

            await Task.Delay(1200);
            operationManager.Stop();

            await operationManager.SessionCompletion;

            var session = operationManager.SessionWithOperations;
            session.Status.Should().Be(OperationStatus.Finished);

            var metrics = session.GetMetrics();
            metrics.OperationsCount.Should().Be(20);
            metrics.FinishedCount.Should().Be(8);
            metrics.SuccessCount.Should().Be(4);
            metrics.ErrorCount.Should().Be(4);
            metrics.InProgressCount.Should().Be(0);
        }

        [Fact]
        public void Metrics()
        {
            IMetrics metrics = AppMetrics.CreateDefaultBuilder().Build();

            CounterOptions counterOptions = new CounterOptions()
            {
                Context = "App",
                Name = "Counter",
            };
            metrics.Measure.Counter.Increment(counterOptions);

            MetricsContextValueSource context = metrics.Snapshot.GetForContext("App");
            CounterValue counterValue1 = context.Counters.First().Value;
        }

        private IOperationManager<SessionState, TaskState> CreateOperationManager()
        {
            return new SessionBuilder<SessionState, TaskState>()
                .ConfigureSessionManager(managerConfiguration => managerConfiguration.MaxConcurrencyLevel = 4)
                .WithLogging(LoggerFactory)
                .CreateOperationManager(OperationId.CreateWithGuid(), new SessionState());
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

        public int? Result { get; set; }

        public TaskState()
        {
        }

        public TaskState(int number, int? result = null)
        {
            Number = number;
            Result = result;
        }

        public TaskState WithResult(int result)
        {
            return new TaskState(Number, result);
        }
    }

    public class MultiplyByTwo : IOperationExecutor<SessionState, TaskState>
    {
        /// <inheritdoc />
        public async Task<IOperation<TaskState>> ExecuteAsync(ISession<SessionState> session, IOperation<TaskState> operation, CancellationToken cancellation = default)
        {
            // Sample of mutable state
            operation.State.Result = operation.State.Number * 2;

            // long work imitation
            await Task.Delay(1000, cancellation);

            // Session can be accessed
            session.Messages.Add(new Message($"Input: {operation.State.Number}, Result: {operation.State.Result}"));

            // the same operation
            return operation;
        }
    }

    public class MultiplyByTwoExt : IOperationExecutorExtended<SessionState, TaskState>
    {
        /// <inheritdoc />
        public async Task ExecuteAsync(OperationExecutionContext<SessionState, TaskState> context)
        {
            var operation = context.Operation;

            // Sample of immutable state
            context.NewState = operation.State.WithResult(operation.State.Number * 2);

            // long work imitation
            await Task.Delay(100, context.Cancellation);

            // example of child activity
            using (context.Tracer.StartActivity("db"))
            {
                await Task.Delay(200);
            }

            using (context.Tracer.StartActivity("ext_api"))
            {
                await Task.Delay(300);
            }

            await Task.Delay(100, context.Cancellation);

            // Session can be accessed
            context.Session.Messages.Add(new Message($"Input: {operation.State.Number}, Result: {operation.State.Result}"));
        }
    }
}
