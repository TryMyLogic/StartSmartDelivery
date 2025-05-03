using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
            //_managementForm.DeliveryManagementFormRequested += (s, e) => SetManagementType<DeliveriesDTO>();
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

                if (_currentPresenter != null)
                {
                    _logger.LogInformation("Disposing old presenter for type {Type}", _currentPresenter.GetType().Name);
                    if (_currentPresenter is IDisposable disposablePresenter)
                    {
                        disposablePresenter.Dispose();
                    }
                }
                _currentPresenter = null;

                _managementForm.DgvMain.Columns.Clear();
                _managementForm.DgvMain.DataSource = null;

                _currentPresenter = _formFactory.CreatePresenter<T>(_managementForm);
                _logger.LogInformation("New presenter created: {PresenterType}", _currentPresenter.GetType().Name);
                _logger.LogInformation("New presenter created: {PresenterType}", _currentPresenter.GetType());

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
            SetManagementType<DriversDTO>();
            Application.Run(_managementForm);
        }
    }
}
