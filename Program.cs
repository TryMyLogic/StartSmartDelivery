using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.Generics;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

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
            GenericManagementForm managementForm = serviceRegistry.GetRequiredService<GenericManagementForm>();
            ILogger<GenericManagementPresenter<DriversDTO>> logger = serviceRegistry.GetRequiredService<ILogger<GenericManagementPresenter<DriversDTO>>>();
            ILogger<GenericDataFormTemplate> dataFormLogger = serviceRegistry.GetRequiredService<ILogger<GenericDataFormTemplate>>();
            ILogger<GenericDataFormPresenter<DriversDTO>> dataFormPresenterLogger = serviceRegistry.GetRequiredService<ILogger<GenericDataFormPresenter<DriversDTO>>>();
            GenericManagementModel<DriversDTO> model = serviceRegistry.GetRequiredService<GenericManagementModel<DriversDTO>>();
            TableConfig tableConfig = serviceRegistry.GetRequiredService<TableConfig>();
            IRepository<DriversDTO> repository = serviceRegistry.GetRequiredService<IRepository<DriversDTO>>();
            _ = new GenericManagementPresenter<DriversDTO>(managementForm, model, tableConfig, repository, logger, dataFormLogger, dataFormPresenterLogger);

            Application.Run(managementForm);
        }
    }
}
