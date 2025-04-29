using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents;

namespace StartSmartDeliveryForm.PresentationLayer
{
    public class ApplicationCoordinator
    {
        private readonly FormFactory _formFactory;
        private readonly ManagementForm _managementForm;
        private object? _currentPresenter;
        private readonly ILogger<ApplicationCoordinator> _logger;

        public ApplicationCoordinator(FormFactory formFactory, ILogger<ApplicationCoordinator> logger)
        {
            _formFactory = formFactory ?? throw new ArgumentNullException(nameof(formFactory));
            _managementForm = _formFactory.CreateForm("ManagementForm") as ManagementForm ?? throw new InvalidOperationException("Failed to create ManagementForm");
            _logger = logger ?? NullLogger<ApplicationCoordinator>.Instance;
            WireUpEvents();
        }

        private void WireUpEvents()
        {
            _managementForm.VehicleManagementFormRequested += (s, e) => SetManagementType<VehiclesDTO>();
            _managementForm.DriverManagementFormRequested += (s, e) => SetManagementType<DriversDTO>();
            _managementForm.DeliveryManagementFormRequested += (s, e) => SetManagementType<DeliveriesDTO>();
            //_managementForm.DashboardFormRequested += HandleDashboardFormRequested;
        }

        public void SetManagementType<T>() where T : class, new()
        {
            _logger.LogInformation("Switching to management type: {DTOType}", typeof(T).Name);
            try
            {
                if (_currentPresenter != null && _currentPresenter.GetType() == typeof(ManagementPresenter<T>))
                {
                    _logger.LogInformation("Management type switch was attempted but current form is already of the target type");
                    return;
                }

                _currentPresenter = _formFactory.CreatePresenter<T>(_managementForm);

                if (_managementForm.FirstLoad == true)
                    _managementForm.InvokeFormLoadOccurred(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to switch to management type: {DTOType}", typeof(T).Name);
                _managementForm.ShowMessageBox($"Error switching to {typeof(T).Name}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Start()
        {
            _logger.LogInformation("Starting Application");
            SetManagementType<DeliveriesDTO>();
            Application.Run(_managementForm);
        }
    }
}
