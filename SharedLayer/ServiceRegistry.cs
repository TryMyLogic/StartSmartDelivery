using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using Serilog.Sinks.MSSqlServer;
using System.Data;
using Serilog.Events;


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
           .WriteTo.File(logFilePath,
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 7,
                  fileSizeLimitBytes: 104857600, // 100MB
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}").ReadFrom.Configuration(configuration)
            .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Debug)

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


            // Dependency injection container
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)  // Inject configuration globally
                .AddSingleton<string>(selectedConnectionString!) // Makes con string available for tests
                .AddLogging(builder => builder.AddSerilog(dispose: true))
                .AddScoped<DriversDAO>()
                .AddScoped<PaginationManager>()
                .AddScoped<DriverManagementForm>()
                .AddScoped<PrintDriverDataForm>()
                .BuildServiceProvider();

            GlobalConstants.Configuration = configuration;

            return serviceProvider;
        }
    }
}
