using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Presenters;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm
{
    internal static class Program
    {

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            IServiceProvider serviceRegistry = ServiceRegistry.RegisterServices();

            // IoC in action
            DriverManagementForm driverManagementForm = serviceRegistry.GetRequiredService<DriverManagementForm>();
            ILogger<DriverManagementFormPresenter> logger = serviceRegistry.GetRequiredService<ILogger<DriverManagementFormPresenter>>();
            DriverManagementModel model = serviceRegistry.GetRequiredService<DriverManagementModel>();
            DriversDAO dao = serviceRegistry.GetRequiredService<DriversDAO>();
            DriverManagementFormPresenter presenter = new(driverManagementForm, model, dao, logger);

            Application.Run(driverManagementForm);
        }
    }
}
