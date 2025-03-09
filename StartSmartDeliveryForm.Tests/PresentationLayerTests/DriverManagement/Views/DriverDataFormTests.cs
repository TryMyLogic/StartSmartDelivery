using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Views;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement.Views
{
    public class DriverDataFormTests(ITestOutputHelper output) : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<DriverDataForm> _testLogger = SharedFunctions.CreateTestLogger<DriverDataForm>(output);
        private DriverDataForm? _driverDataForm;

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", LicenseType.Code8, false)]
        [InlineData(2, "Jane", "Smith", "EMP002", LicenseType.Code8, true)]
        [InlineData(3, "Jim", "Brown", "EMP003", LicenseType.Code10, false)]
        [InlineData(4, "Jake", "White", "EMP004", LicenseType.Code10, true)]
        [InlineData(5, "Jill", "Green", "EMP005", LicenseType.Code14, false)]
        [InlineData(6, "Jack", "Black", "EMP006", LicenseType.Code14, true)]
        public void InitializeEditing_ProperlySetsFieldsValues(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            // Arrange
            _driverDataForm = new DriverDataForm(_testLogger);

            DriversDTO mockDriver = new(
                DriverID: DriverID,
                Name: Name,
                Surname: Surname,
                EmployeeNo: EmployeeNo,
                LicenseType: LicenseType,
                Availability: Availability
            );

            // Act
            _driverDataForm.InitializeEditing(mockDriver);

            // Assert
            Assert.Equal(DriverID, _driverDataForm.DriverID);
            Assert.Equal(Name, _driverDataForm.DriverName);
            Assert.Equal(Surname, _driverDataForm.DriverSurname);
            Assert.Equal(EmployeeNo, _driverDataForm.DriverEmployeeNo);
            Assert.Equal(LicenseType, _driverDataForm.DriverLicenseType);
            Assert.Equal(Availability, _driverDataForm.DriverAvailability);
        }

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", LicenseType.Code8, false)]
        [InlineData(2, "Jane", "Smith", "EMP002", LicenseType.Code8, true)]
        [InlineData(3, "Jim", "Brown", "EMP003", LicenseType.Code10, false)]
        [InlineData(4, "Jake", "White", "EMP004", LicenseType.Code10, true)]
        [InlineData(5, "Jill", "Green", "EMP005", LicenseType.Code14, false)]
        [InlineData(6, "Jack", "Black", "EMP006", LicenseType.Code14, true)]
        public void ClearData_ClearsDataFormFields(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            // Arrange
            _driverDataForm = new DriverDataForm(_testLogger);

            DriversDTO mockDriver = new(
                DriverID: DriverID,
                Name: Name,
                Surname: Surname,
                EmployeeNo: EmployeeNo,
                LicenseType: LicenseType,
                Availability: Availability
            );

            _driverDataForm.InitializeEditing(mockDriver);

            // Act
            _driverDataForm.ClearData();

            // Assert
            Assert.Equal(0, _driverDataForm.DriverID);
            Assert.Equal("", _driverDataForm.DriverName);
            Assert.Equal("", _driverDataForm.DriverSurname);
            Assert.Equal("", _driverDataForm.DriverEmployeeNo);
            // DriverDataForm clears selection to null, but DriverLicenseType defaults to Code8 (0) due to a null check in getter
            Assert.Equal(0, (int)_driverDataForm.DriverLicenseType);
            Assert.True(_driverDataForm.DriverAvailability);
        }

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", LicenseType.Code8, false)]
        [InlineData(2, "Jane", "Smith", "EMP002", LicenseType.Code8, true)]
        [InlineData(3, "Jim", "Brown", "EMP003", LicenseType.Code10, false)]
        [InlineData(4, "Jake", "White", "EMP004", LicenseType.Code10, true)]
        [InlineData(5, "Jill", "Green", "EMP005", LicenseType.Code14, false)]
        [InlineData(6, "Jack", "Black", "EMP006", LicenseType.Code14, true)]
        public void GetData_GetsDataFromDataFormFields(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            // Arrange
            _driverDataForm = new DriverDataForm(_testLogger);

            DriversDTO mockDriver = new(
                DriverID: DriverID,
                Name: Name,
                Surname: Surname,
                EmployeeNo: EmployeeNo,
                LicenseType: LicenseType,
                Availability: Availability
            );

            _driverDataForm.InitializeEditing(mockDriver); 

            // Act
            DriversDTO driver = _driverDataForm.GetData();

            // Assert
            Assert.Equal(DriverID, driver.DriverID);
            Assert.Equal(Name, driver.Name);
            Assert.Equal(Surname, driver.Surname);
            Assert.Equal(EmployeeNo, driver.EmployeeNo);
            Assert.Equal(LicenseType, driver.LicenseType);
            Assert.Equal(Availability, driver.Availability);
        }
    }
}
