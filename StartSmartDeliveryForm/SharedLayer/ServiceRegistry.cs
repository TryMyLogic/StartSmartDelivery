using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Serilog;
using Serilog.Events;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.DataLayer.Repositories;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.PresentationLayer.DataFormComponents;
using StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;


namespace StartSmartDeliveryForm.SharedLayer
{

    public static class ServiceRegistry
    {
        /* 
        This class allows implementation of Inversion of Control (IoC) and Dependency Injection (DI).
   
        Inversion of Control (IoC) is a design principle where the control of object creation 
        is shifted from the class to a container or framework. This helps decouple components.
   
        Dependency Injection (DI) is a technique where dependencies are provided to a class rather
        than the class creating them itself. This improves flexibility, testability, and maintainability.
        */
        public static IServiceProvider RegisterServices(string conStringName = "StartSmartDB")
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string? selectedConnectionString = configuration.GetConnectionString(conStringName);

            string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            string logFilePath = Path.Combine(logsDirectory, "RuntimeLog_.txt");

            // Serilog setup
            Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .WriteTo.File(logFilePath,
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 7,
                  fileSizeLimitBytes: 104857600, // 100MB
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}").ReadFrom.Configuration(configuration)
            .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Debug)
            .MinimumLevel.Override("Polly", LogEventLevel.Warning)

            // Filter for SQL logs (Not fully tested yet, will do later)
            //.WriteTo.Logger(lc => lc
            //.Filter.ByIncludingOnly(logEvent =>
            //logEvent.Properties.TryGetValue("SecurityAudit", out Serilog.Events.LogEventPropertyValue? value) &&
            //value.ToString().Trim('"') == "SQL")
            //.WriteTo.MSSqlServer(
            //connectionString: selectedConnectionString!,
            //sinkOptions: new MSSqlServerSinkOptions
            //{
            //    AutoCreateSqlTable = false,
            //    TableName = "SecurityAudit"
            //},
            //    columnOptions: new ColumnOptions
            //    {
            //        AdditionalColumns =
            //        [
            //        new("UserID", SqlDbType.NVarChar),
            //        new("ActionType", SqlDbType.NVarChar),
            //        new("TableAffected", SqlDbType.NVarChar),
            //        new("ChangeSummary", SqlDbType.NVarChar),
            //        new("ComputerName", SqlDbType.NVarChar)
            //        ]
            //    }
            //))

            .CreateLogger();

            // Example of handling specific result. Will be used to specify transient error codes. Non-transient will be handled seperately
            // .HandleResult(response => response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            // See https://learn.microsoft.com/en-us/sql/connect/ado-net/step-4-connect-resiliently-sql-ado-net?view=sql-server-ver16

            // Dependency injection container
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)  // Inject configuration globally
                .AddSingleton<string>(selectedConnectionString!) // Makes con string available for tests
                .AddLogging(builder => builder.AddSerilog(dispose: true))
                .AddSingleton<RetryEventService>()
                .AddResiliencePipeline("sql-retry-pipeline", (builder, context) =>
                {
                    RetryEventService retryEventService = context.ServiceProvider.GetRequiredService<RetryEventService>();
                    Microsoft.Extensions.Logging.ILogger RetryLogger = context.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SQL-Retry");
                    int MaxRetryAttemptsAllowed = 3;

                    builder.AddTimeout(TimeSpan.FromSeconds(60));

                    builder.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = MaxRetryAttemptsAllowed,
                        Delay = TimeSpan.FromSeconds(5),
                        ShouldHandle = new Func<RetryPredicateArguments<object>, ValueTask<bool>>(args =>
                        {
                            return new ValueTask<bool>(args.Outcome.Exception is SqlException);
                        }),
                        OnRetry = args =>
                        {
                            if (args.Outcome.Exception is not null)
                            {
                                RetryLogger.LogError("Retrying connection to database. Attempt {AttemptNumber} of {MaxRetryAttempts}. Retrying in {DelaySeconds} seconds. Exception: {ExceptionMessage}.",
                                args.AttemptNumber + 1, // Is 0 indexed. Adding 1 for display purposes
                                MaxRetryAttemptsAllowed,
                                args.RetryDelay,
                                args.Outcome.Exception.Message
                                );

                                retryEventService.OnRetryOccurred(
                                args.AttemptNumber + 1,
                                MaxRetryAttemptsAllowed,
                                args.RetryDelay,
                                args.Outcome.Exception.Message
                                );
                            }

                            return default;
                        }
                    });

                })

                .AddSingleton<ApplicationCoordinator>()
                .AddSingleton<FormFactory>()
                .AddSingleton<TableConfigResolver>()
                .AddTransient(typeof(ManagementPresenter<>))


                .AddScoped<ManagementForm>()

                // DriverManagementForm
                .AddScoped<IRepository<DriversDTO>, Repository<DriversDTO>>()
                .AddScoped<PaginationManager<DriversDTO>>()
                .AddScoped<IManagementModel<DriversDTO>, ManagementModel<DriversDTO>>()
                .AddScoped<ManagementModel<DriversDTO>>()
                .AddTransient<ManagementPresenter<DriversDTO>>()
                .AddScoped<IRepository<DriversDTO>, Repository<DriversDTO>>()

                // VehicleManagementForm
                .AddScoped<IRepository<VehiclesDTO>, Repository<VehiclesDTO>>()
                .AddScoped<PaginationManager<VehiclesDTO>>()
                .AddScoped<IManagementModel<VehiclesDTO>, ManagementModel<VehiclesDTO>>()
                .AddScoped<ManagementModel<VehiclesDTO>>()
                .AddTransient<ManagementPresenter<VehiclesDTO>>()
                .AddScoped<IRepository<VehiclesDTO>, Repository<VehiclesDTO>>()

                // DeliveryManagementForm
                .AddScoped<IRepository<DeliveriesDTO>, DeliveriesRepository>()
                .AddScoped<PaginationManager<DeliveriesDTO>>()
                .AddScoped<IManagementModel<DeliveriesDTO>, ManagementModel<DeliveriesDTO>>()
                .AddScoped<ManagementModel<DeliveriesDTO>>()
                .AddTransient<ManagementPresenter<DeliveriesDTO>>()
                .AddScoped<IRepository<DeliveriesDTO>, DeliveriesRepository>()

                // DataForm
                .AddScoped<IDataForm, DataForm>()
                .AddScoped(typeof(IDataModel<>), typeof(DataModel<>))

                .BuildServiceProvider();

            GlobalConstants.Configuration = configuration;

            return serviceProvider;
        }
    }
}
