using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.DataLayer.Repositories;
using StartSmartDeliveryForm.PresentationLayer.DataFormComponents;
using StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents;
using StartSmartDeliveryForm.PresentationLayer.PrintDataFormComponents;

namespace StartSmartDeliveryForm.PresentationLayer
{
    public class FormFactory(IServiceProvider serviceProvider, ILogger<FormFactory> logger)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly ILogger<FormFactory> _logger = logger ?? NullLogger<FormFactory>.Instance;

        public Form CreateForm(string formType, string? subType = null)
        {
            _logger.LogDebug("Creating form of type {FormType} with subType {SubType}", formType, subType ?? "none");

            return formType switch
            {
                "ManagementForm" => CreateManagementForm(),
                // "DashboardForm" => CreateDashboardForm(),
                _ => throw new ArgumentException($"Unknown form type: {formType}", nameof(formType))
            };
        }

        //private DashboardForm CreateDashboardForm()
        //{
        //    try
        //    {
        //        DashboardForm form = _serviceProvider.GetRequiredService<DashboardForm>();


        //        _logger.LogInformation("Successfully created DashboardForm");
        //        return form;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to create DashboardForm");
        //        throw;
        //    }
        //}

        private ManagementForm CreateManagementForm()
        {
            ManagementForm form = _serviceProvider.GetRequiredService<ManagementForm>();
            _logger.LogInformation("Successfully created ManagementForm");
            return form;
        }

        public ManagementPresenter<T> CreatePresenter<T>(IManagementForm form) where T : class, new()
        {
            try
            {
                IManagementModel<T> model = _serviceProvider.GetRequiredService<IManagementModel<T>>();
                IRepository<T> repository = _serviceProvider.GetRequiredService<IRepository<T>>();
                ILogger<ManagementPresenter<T>> logger = _serviceProvider.GetRequiredService<ILogger<ManagementPresenter<T>>>();
        
           

                IDataForm dataForm = _serviceProvider.GetRequiredService<IDataForm>();
                IDataModel<T> dataModel = _serviceProvider.GetRequiredService<IDataModel<T>>();
                ILogger<IDataForm> dataFormLogger = _serviceProvider.GetRequiredService<ILogger<IDataForm>>();
                ILogger<IDataModel<T>> dataModelLogger = _serviceProvider.GetRequiredService<ILogger<IDataModel<T>>>();
                ILogger<IDataPresenter<T>> dataPresenterLogger = _serviceProvider.GetRequiredService<ILogger<IDataPresenter<T>>>();

                ILogger<PrintDataForm> printDataFormLogger = _serviceProvider.GetRequiredService<ILogger<PrintDataForm>>();
                ILogger<PrintDataPresenter<T>> printDataPresenterLogger = _serviceProvider.GetRequiredService<ILogger<PrintDataPresenter<T>>>();

                ManagementPresenter<T> presenter = new(
                    form,
                    model,
                    repository,
                    dataForm,
                    dataModel,
                    logger
                    //
                    //
                    //printDataFormLogger,
                    //printDataPresenterLogger
                );

                _logger.LogInformation("Successfully created ManagementPresenter for {DTOType}", typeof(T).Name);
                return presenter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ManagementPresenter for {DTOType}", typeof(T).Name);
                throw;
            }
        }
    }
}
