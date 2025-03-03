using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models;
using StartSmartDeliveryForm.PresentationLayer.TemplatePresenters;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Presenters
{
    internal class DriverDataFormPresenter : DataFormPresenterTemplate
    {
        private readonly IDriverDataForm _driverDataForm;
        private readonly DriversDAO _driversDAO;
        private readonly DataFormValidator _dataFormValidator;
        private readonly ILogger<DriverDataFormPresenter> _logger;

        public DriverDataFormPresenter(IDriverDataForm dataForm, DriversDAO driversDAO, DataFormValidator? dataFormValidator, ILogger<DriverDataFormPresenter>? logger = null) : base(dataForm, logger)
        {
            _driverDataForm = dataForm;
            _driversDAO = driversDAO;
            _dataFormValidator = dataFormValidator ?? new DataFormValidator();
            _logger = logger ?? NullLogger<DriverDataFormPresenter>.Instance;

            _dataFormValidator.RequestMessageBox += _driverDataForm.ShowMessageBox;
        }

        public IDriverDataForm GetDataForm()
        {
            return _driverDataForm;
        }

        protected override async Task<bool> ValidFormAsync()
        {
            if (!_dataFormValidator.IsValidString(_driverDataForm.Name, "Name")) return false;
            if (!_dataFormValidator.IsValidString(_driverDataForm.Surname, "Surname")) return false;
            if (!_dataFormValidator.IsValidString(_driverDataForm.EmployeeNo, "EmployeeNo"))
            {
                Driver driver = new(_driversDAO);

                // Only check uniqueness if the field isn't empty && Mode is not edit
                if (_driverDataForm.Mode == FormMode.Add && !(await driver.IsEmployeeNoUnique(_driverDataForm.EmployeeNo)))
                {
                    _driverDataForm.ShowMessageBox("Employee No is not unique.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            if (!_dataFormValidator.IsValidEnumValue<LicenseType>(_driverDataForm.LicenseType.ToString())) return false;

            if (!_dataFormValidator.IsValidBoolValue(_driverDataForm.Availability.ToString())) return false;
            
            return true;
        }
    }
}
