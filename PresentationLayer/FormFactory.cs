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
                "ManagementForm" => CreateManagementForm(subType),
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

        private ManagementForm CreateManagementForm(string? subType)
        {
            if (string.IsNullOrEmpty(subType))
            {
                _logger.LogError("A SubType is required for ManagementForm creation.");
                throw new ArgumentException("A SubType is required for ManagementForm.", nameof(subType));
            }

            _logger.LogDebug("Creating ManagementForm with subType {SubType}", subType);

            return subType switch
            {
                "DriverManagementForm" => CreateManagementForm<DriversDTO>(),
                "VehicleManagementForm" => CreateManagementForm<VehiclesDTO>(),
                //   "DeliveryManagementForm" => CreateManagementForm<DeliveriesDTO>(),
                _ => throw new ArgumentException($"Unknown ManagementForm SubType: {subType}", nameof(subType))
            };
        }

        private ManagementForm CreateManagementForm<T>() where T : class, new()
        {
            try
            {
                ManagementForm form = _serviceProvider.GetRequiredService<ManagementForm>();
                ManagementModel<T> model = _serviceProvider.GetRequiredService<ManagementModel<T>>();
                IRepository<T> repository = _serviceProvider.GetRequiredService<IRepository<T>>();
                ILogger<ManagementPresenter<T>> logger = _serviceProvider.GetRequiredService<ILogger<ManagementPresenter<T>>>();
                ILogger<DataForm> dataFormLogger = _serviceProvider.GetRequiredService<ILogger<DataForm>>();
                ILogger<DataFormPresenter<T>> dataFormPresenterLogger = _serviceProvider.GetRequiredService<ILogger<DataFormPresenter<T>>>();

                _ = new ManagementPresenter<T>(
                    form, model, repository, this, logger, dataFormLogger, dataFormPresenterLogger
                );

                _logger.LogInformation("Successfully created ManagementForm for {DTOType}", typeof(T).Name);
                return form;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ManagementForm for {DTOType}", typeof(T).Name);
                throw;
            }
        }


    }
}
