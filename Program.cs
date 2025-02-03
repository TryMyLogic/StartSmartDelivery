using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            FormConsole.Instance.Show();

            IServiceProvider serviceRegistry = ServiceRegistry.RegisterServices();

            // IoC in action
            DriverManagementForm driverManagementForm = serviceRegistry.GetRequiredService<DriverManagementForm>();

            Application.Run(driverManagementForm);
        }
    }
}
