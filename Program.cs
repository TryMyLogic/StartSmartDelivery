using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.PresentationLayer.TemplatePresenters;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            IServiceProvider serviceRegistry = ServiceRegistry.RegisterServices();
            ServiceProvider = serviceRegistry;

            // IoC in action
            DriverManagementForm driverManagementForm = serviceRegistry.GetRequiredService<DriverManagementForm>();
            ManagementFormPresenterTemplate presenter = new(driverManagementForm);

            Application.Run(driverManagementForm);
        }
    }
}
