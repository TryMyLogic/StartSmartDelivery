using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DataFormComponents;
using StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents;
using StartSmartDeliveryForm.SharedLayer;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;
using StartSmartDeliveryForm.DataLayer.Repositories;
using StartSmartDeliveryForm.PresentationLayer;

namespace StartSmartDeliveryForm
{
    internal static class Program
    {

        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            IServiceProvider serviceRegistry = ServiceRegistry.RegisterServices();

            // IoC in action
            FormFactory formFactory = serviceRegistry.GetRequiredService<FormFactory>();

            Form managementForm = formFactory.CreateForm("ManagementForm", "DriverManagementForm");

            Application.Run(managementForm);
        }
    }
}
