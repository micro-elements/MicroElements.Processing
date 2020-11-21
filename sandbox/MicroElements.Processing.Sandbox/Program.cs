
using System.Diagnostics;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Trace;

public class Program
{
    private static readonly ActivitySource MyActivitySource = new ActivitySource(
        "MicroElements.Processing");

    public static async Task Main()
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetSampler(new AlwaysOnSampler())
            .AddSource(MyActivitySource.Name)
            .AddConsoleExporter()
            .Build();

        using (Activity? sayHelloActivity = MyActivitySource.StartActivity("SayHello"))
        {
            sayHelloActivity?.SetTag("foo", 1);
            sayHelloActivity?.SetTag("bar", "Hello, World!");
            await Task.Delay(1000);

            using var inner = MyActivitySource.StartActivity("Inner", ActivityKind.Internal, parentId: sayHelloActivity?.Id!);
            await Task.Delay(1000);
        }
    }
}
