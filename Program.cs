using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DataFormComponents;
using StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents;
using StartSmartDeliveryForm.SharedLayer;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;
using StartSmartDeliveryForm.DataLayer.Repositories;

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
            ManagementForm managementForm = serviceRegistry.GetRequiredService<ManagementForm>();
            ILogger<ManagementPresenter<DriversDTO>> logger = serviceRegistry.GetRequiredService<ILogger<ManagementPresenter<DriversDTO>>>();
            ILogger<DataForm> dataFormLogger = serviceRegistry.GetRequiredService<ILogger<DataForm>>();
            ILogger<DataFormPresenter<DriversDTO>> dataFormPresenterLogger = serviceRegistry.GetRequiredService<ILogger<DataFormPresenter<DriversDTO>>>();
            ManagementModel<DriversDTO> model = serviceRegistry.GetRequiredService<ManagementModel<DriversDTO>>();
            TableConfig tableConfig = serviceRegistry.GetRequiredService<TableConfig>();
            IRepository<DriversDTO> repository = serviceRegistry.GetRequiredService<IRepository<DriversDTO>>();
            _ = new ManagementPresenter<DriversDTO>(managementForm, model, tableConfig, repository, logger, dataFormLogger, dataFormPresenterLogger);

            Application.Run(managementForm);
        }
    }
}
