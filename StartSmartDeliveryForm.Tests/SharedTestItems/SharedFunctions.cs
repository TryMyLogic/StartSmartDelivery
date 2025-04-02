using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.SharedTestItems
{
    internal class SharedFunctions
    {
        public static ILogger<T> CreateTestLogger<T>(ITestOutputHelper output)
        {
            Logger serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(output)
                .CreateLogger();

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(serilogLogger);
            });

            return loggerFactory.CreateLogger<T>();
        }

        // Used to create a new memory sink instance for each test
        public static (ILogger<T> Logger, InMemorySink MemorySink) CreateMemorySinkLogger<T>()
        {
            InMemorySink memorySink = new();

            Logger serilogMemoryLogger = new LoggerConfiguration()
               .WriteTo.Sink(memorySink)
               .CreateLogger();

            ILoggerFactory memoryLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(serilogMemoryLogger);
            });

            ILogger<T> logger = memoryLoggerFactory.CreateLogger<T>();

            return (logger, memorySink);
        }

        public static void AssertSingleLogEvent(InMemorySink? memorySink, LogEventLevel expectedLevel, string expectedMessage)
        {
            if (memorySink != null)
            {
                if (memorySink.LogEvents.Any())
                {
                    List<LogEvent> memoryLog = memorySink.LogEvents.ToList();
                    Assert.Single(memoryLog);
                    Assert.Equal(expectedLevel, memoryLog[0].Level);
                    Assert.Contains(expectedMessage, memoryLog[0].RenderMessage());
                }
                else
                {
                    Assert.Fail("No log events found.");
                }
            }
            else
            {
                Assert.Fail("Memory sink is null.");
            }
        }
    }
}
