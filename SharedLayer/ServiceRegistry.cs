using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;

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

            // Dependency injection container
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)  // Inject configuration globally
                .AddSingleton<string>(selectedConnectionString!) // Makes con string available for tests
                .AddScoped<DriversDAO>(provider => new DriversDAO(configuration, selectedConnectionString!))
                .AddScoped<PaginationManager>()
                .AddScoped<DriverManagementForm>()
                .AddScoped<PrintDriverDataForm>()
                .BuildServiceProvider();

            GlobalConstants.Configuration = configuration;

            return serviceProvider;
        }
    }
}
