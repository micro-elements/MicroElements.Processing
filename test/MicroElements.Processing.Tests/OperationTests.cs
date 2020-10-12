using FluentAssertions;
using MicroElements.Metadata;
using MicroElements.Processing.TaskManager;
using Xunit;

namespace MicroElements.Processing.Tests
{
    public class OperationTests
    {
        public class Base
        {
            public string A { get; }

            public Base(string a)
            {
                A = a;
            }
        }

        public class Derived : Base
        {
            public string B { get; }

            public Derived(string a, string b) : base(a)
            {
                B = b;
            }
        }

        [Fact]
        public void operation_should_be_covariant()
        {
            IOperation<Derived> operation1 = Operation.CreateNotStarted("1", new Derived("a", "b"));
            IOperation<Base> operation2 = Operation.CreateNotStarted("2", new Derived("a", "b"));

            operation2.State.A.Should().Be(operation1.State.A);
        }

        [Fact]
        public void operation_with_metadata()
        {
            IOperation<int> operation = Operation.CreateNotStarted("1", 1,
                metadata: new MutablePropertyContainer().WithValue("AttachedProperty", "Value"));

            operation.GetMetadata<string>("AttachedProperty").Should().Be("Value");

            var updated = operation.With(status: OperationStatus.InProgress);
            updated.GetMetadata<string>("AttachedProperty").Should().Be("Value");
        }
    }
}
