using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Presenters;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement.Presenters
{
    public class DriverDataFormPresenterTests
    {
        private readonly ILogger<DriverDataFormPresenter> _testLogger;
        private readonly DriversDAO _driversDAO;
        private readonly DataFormValidator _dataFormValidator;
        private readonly DriverDataForm _driverDataForm;
        private DriverDataFormPresenter? _driverDataFormPresenter;

        public DriverDataFormPresenterTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            ILogger<DriverDataForm> _dataFormTestLogger = SharedFunctions.CreateTestLogger<DriverDataForm>(output);
            _driverDataForm = new(_dataFormTestLogger, new NoMessageBox());
            _driversDAO = fixture.DriversDAO;
            _dataFormValidator = new DataFormValidator();
            _testLogger = SharedFunctions.CreateTestLogger<DriverDataFormPresenter>(output);
        }

    }
}
