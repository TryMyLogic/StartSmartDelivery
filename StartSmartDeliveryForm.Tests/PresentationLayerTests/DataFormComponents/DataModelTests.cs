using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.DataLayer.Repositories;
using StartSmartDeliveryForm.PresentationLayer.DataFormComponents;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DataFormComponents
{
    public class DataModelTests(DatabaseFixture fixture, ITestOutputHelper output) : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<DataModel<DriversDTO>> _testLogger = SharedFunctions.CreateTestLogger<DataModel<DriversDTO>>(output);
        private readonly IRepository<DriversDTO> _repository = fixture.DriversRepository;
        private DataForm? _dataForm;
        private DataPresenter<DriversDTO>? _dataPresenter;
        private DataModel<DriversDTO>? _dataModel;

        [Fact]
        public void GenerateControls_GeneratesCorrectControls_FromTableConfig()
        {
            // Arrange
            _dataModel = new(_repository, _testLogger);

            Dictionary<string, (Label Label, Control Control)> expectedControlsLayout = new()
            {
            { "DriverID", (new Label { Text = "DriverID" }, new TextBox { Name = "txtDriverID" }) },
            { "Name", (new Label { Text = "Name" }, new TextBox { Name = "txtName" }) },
            { "Surname", (new Label { Text = "Surname" }, new TextBox { Name = "txtSurname" }) },
            { "EmployeeNo", (new Label { Text = "EmployeeNo" }, new TextBox { Name = "txtEmployeeNo" }) },
            { "LicenseType", (new Label { Text = "LicenseType" },new ComboBox{Name = "cboLicenseType",Items = { LicenseType.Code8, LicenseType.Code10, LicenseType.Code14 }})},
            { "Availability", (new Label { Text = "Availability" }, new ComboBox{Name = "cboAvailability",Items = { false, true }})}
            };

            // Act
            Dictionary<string, (Label Label, Control Control)> result = _dataModel.GenerateControls();

            // Assert
            foreach (string key in expectedControlsLayout.Keys)
            {
                (Label expectedLabel, Control expectedControl) = expectedControlsLayout[key];
                (Label actualLabel, Control actualControl) = result[key];

                Assert.Equal(expectedLabel.Text, actualLabel.Text);

                Assert.Equal(expectedControl.GetType(), actualControl.GetType());

                if (expectedControl is TextBox expectedText && actualControl is TextBox actualText)
                {
                    Assert.Equal(expectedText.Text, actualText.Text);
                }
                else if (expectedControl is ComboBox expectedCombo && actualControl is ComboBox actualCombo)
                {
                    Assert.Equal(expectedCombo.Items.Count, actualCombo.Items.Count);
                    for (int i = 0; i < expectedCombo.Items.Count; i++)
                    {
                        Assert.Equal(expectedCombo.Items[i]?.ToString(), actualCombo.Items[i]?.ToString());
                    }
                }
            }
        }

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", LicenseType.Code8, false)]
        [InlineData(2, "Jane", "Smith", "EMP002", LicenseType.Code8, true)]
        [InlineData(3, "Jim", "Brown", "EMP003", LicenseType.Code10, false)]
        [InlineData(4, "Jake", "White", "EMP004", LicenseType.Code10, true)]
        [InlineData(5, "Jill", "Green", "EMP005", LicenseType.Code14, false)]
        [InlineData(6, "Jack", "Black", "EMP006", LicenseType.Code14, true)]
        public void CreateFromForm_CreatesEntityFromForm(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            // Arrange
            _dataForm = new(null, new NoMessageBox());
            DriversDTO expectedDriver = new(DriverID, Name, Surname, EmployeeNo, LicenseType, Availability);
            _dataModel = new(_repository, _testLogger);
            _dataPresenter = new(_dataForm, _dataModel);
            _dataPresenter.InitializeEditing(expectedDriver);

            // Act
            DriversDTO resultingEntity = _dataModel.CreateFromForm(_dataForm.GetControls());

            // Assert
            Assert.Equal(expectedDriver, resultingEntity);
        }

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", LicenseType.Code8, false)]
        [InlineData(2, "Jane", "Smith", "EMP002", LicenseType.Code8, true)]
        [InlineData(3, "Jim", "Brown", "EMP003", LicenseType.Code10, false)]
        [InlineData(4, "Jake", "White", "EMP004", LicenseType.Code10, true)]
        [InlineData(5, "Jill", "Green", "EMP005", LicenseType.Code14, false)]
        [InlineData(6, "Jack", "Black", "EMP006", LicenseType.Code14, true)]
        public void MapToForm_MapsEntityValues(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            // Arrange
            _dataModel = new(_repository, _testLogger);
            DriversDTO driverToMap = new(DriverID, Name, Surname, EmployeeNo, LicenseType, Availability);

            Dictionary<string, object> expectedResult = new()
            {
            { "DriverID", DriverID },
            { "Name", Name },
            { "Surname", Surname },
            { "EmployeeNo", EmployeeNo },
            { "LicenseType", LicenseType },
            { "Availability", Availability }
            };

            // Act
            Dictionary<string, object> result = _dataModel.MapToForm(driverToMap);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetDefaultValues_ReturnsDictionaryOfDefaultValues()
        {
            // Arrange
            _dataModel = new(_repository, _testLogger);

            Dictionary<string, object?> expectedDefaultValues = new()
            {
            { "DriverID", null },
            { "Name", null },
            { "Surname", null },
            { "EmployeeNo", null },
            { "LicenseType", null },
            { "Availability", true }
            };

            // Act
            Dictionary<string, object?> result = _dataModel.GetDefaultValues();

            // Assert
            Assert.Equal(expectedDefaultValues, result);
        }

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
        public async Task ValidateFormAsync_ReturnsCorrectBool_ForDriver(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability, bool ExpectedResult)
        {
            // Arrange
            _dataForm = new(null, new NoMessageBox());
            DriversDTO Driver = new(DriverID, Name, Surname, EmployeeNo, LicenseType, Availability);
            _dataModel = new(_repository, _testLogger);
            _dataPresenter = new(_dataForm, _dataModel);
            _dataPresenter.InitializeEditing(Driver);

            // Act
            (bool result, _) = await _dataModel.ValidateFormAsync(_dataForm.GetControls());

            // Assert
            Assert.Equal(ExpectedResult, result);
        }
    }
}
