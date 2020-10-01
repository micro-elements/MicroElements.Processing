using System;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Processing.TaskManager;
using Xunit;

namespace MicroElements.Processing.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void OperationId_operations()
        {
            ((Action)(() => new OperationId(null))).Should().Throw<ArgumentNullException>();
            ((Action)(() => _ = default(OperationId).Value)).Should().Throw<NotInitializedException>();
            new OperationId("test").Should().Be(new OperationId("test"));
            new OperationId("test").Should().BeEquivalentTo(new OperationId("test"));
            new OperationId("test").GetHashCode().Should().Be(new OperationId("test").GetHashCode());
        }
    }
}
