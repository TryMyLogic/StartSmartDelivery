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

        public static void AssertSingleLogEvent(InMemorySink? MemorySink, LogEventLevel ExpectedLevel, string ExpectedMessage)
        {
            if (MemorySink != null)
            {
                if (MemorySink.LogEvents.Any())
                {
                    List<LogEvent> memoryLog = MemorySink.LogEvents.ToList();
                    Assert.Equal(ExpectedLevel, memoryLog[0].Level);
                    Assert.Contains(ExpectedMessage, memoryLog[0].RenderMessage());
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

        public static void AssertLogEventContainsMessage(InMemorySink? MemorySink, LogEventLevel ExpectedLevel, string ExpectedMessage)
        {
            if (MemorySink != null)
            {
                if (MemorySink.LogEvents.Any())
                {
                    List<LogEvent> matchingLogEvents = MemorySink.LogEvents
                        .Where(logEvent => logEvent.Level == ExpectedLevel && logEvent.RenderMessage().Contains(ExpectedMessage))
                        .ToList();

                    Assert.NotEmpty(matchingLogEvents);
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
