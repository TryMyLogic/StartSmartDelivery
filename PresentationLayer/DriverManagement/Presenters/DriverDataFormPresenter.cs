﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Views;
using StartSmartDeliveryForm.PresentationLayer.TemplatePresenters;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Presenters
{
    public class DriverDataFormPresenter : DataFormPresenterTemplate
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

        protected override async Task<bool> ValidFormAsync()
        {
            _logger.LogInformation("Validating Form");

            if (!_dataFormValidator.IsValidString(_driverDataForm.DriverName, "Name")) return false;
            if (!_dataFormValidator.IsValidString(_driverDataForm.DriverSurname, "Surname")) return false;
            if (!_dataFormValidator.IsValidString(_driverDataForm.DriverEmployeeNo, "EmployeeNo"))
            {
                Driver driver = new(_driversDAO);

                // Only check uniqueness if the field isn't empty && Mode is not edit
                if (_driverDataForm.Mode == FormMode.Add && !(await driver.IsEmployeeNoUnique(_driverDataForm.DriverEmployeeNo)))
                {
                    _driverDataForm.ShowMessageBox("Employee No is not unique.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            if (!_dataFormValidator.IsValidEnumValue<LicenseType>(_driverDataForm.DriverLicenseType.ToString())) return false;

            if (!_dataFormValidator.IsValidBoolValue(_driverDataForm.DriverAvailability.ToString())) return false;

            return true;
        }
    }
}
