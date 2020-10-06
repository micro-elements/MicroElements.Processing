using MicroElements.Testing.XUnit.Logging;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace MicroElements.Processing.Tests
{
    /// <summary>
    /// XUnit test with ability to use logger.
    /// </summary>
    public abstract class UnitTestWithOutput
    {
        protected ITestOutputHelper TestOutputHelper { get; }
        protected ILoggerFactory LoggerFactory { get; }
        protected ILogger Logger { get; }

        protected UnitTestWithOutput(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            LoggerFactory = TestLoggerFactory.CreateXUnitLoggerProvider(TestOutputHelper);
            Logger = LoggerFactory.CreateLogger(GetType());
        }
    }
}
