using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.Generics;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;
using static StartSmartDeliveryForm.Generics.TableDefinition;

namespace StartSmartDeliveryForm.Tests.GenericTests
{
    public class GenericDataFormPresenterTests(DatabaseFixture fixture, ITestOutputHelper output) : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<GenericDataFormPresenter<DriversDTO>> _testLogger = SharedFunctions.CreateTestLogger<GenericDataFormPresenter<DriversDTO>>(output);
        private readonly IRepository<DriversDTO> _repository = fixture.DriversRepository;
        private readonly GenericDataFormValidator _genericDataFormValidator = new();
        private GenericDataFormTemplate? _genericDataForm;

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", LicenseType.Code8, false, true)]
        [InlineData(2, "Jane", "Smith", "EMP002", LicenseType.Code8, true, true)]
        [InlineData(3, "Jim", "Brown", "EMP003", LicenseType.Code10, false, true)]
        [InlineData(4, "Jake", "White", "EMP004", LicenseType.Code10, true, true)]
        [InlineData(5, "Jill", "Green", "EMP005", LicenseType.Code14, false, true)]
        [InlineData(6, "Jack", "Black", "EMP006", LicenseType.Code14, true, true)]

        // Empty
        [InlineData(1, "", "Doe", "EMP001", LicenseType.Code8, true, false)]
        [InlineData(2, "John", "", "EMP002", LicenseType.Code8, true, false)]
        [InlineData(3, "John", "Doe", "", LicenseType.Code8, true, false)]

        // Non-existent LicenseType
        [InlineData(1, "John", "Doe", "EMP001", (LicenseType)999, false, true)]
        public async Task ValidFormAsync_ReturnsCorrectBool_ForDriver(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability, bool ExpectedResult)
        {
            // Arrange
            _genericDataForm = new(typeof(DriversDTO), TableConfigs.Drivers, null, new NoMessageBox());
            DriversDTO Driver = new(DriverID, Name, Surname, EmployeeNo, LicenseType, Availability);
            _genericDataForm.InitializeEditing(Driver);
            GenericDataFormPresenter<DriversDTO> presenter = new(_genericDataForm, _repository, TableConfigs.Drivers, _genericDataFormValidator);

            // Act
            bool result = await presenter.ValidFormAsync();

            // Assert
            Assert.Equal(ExpectedResult, result);
        }

    }

}
