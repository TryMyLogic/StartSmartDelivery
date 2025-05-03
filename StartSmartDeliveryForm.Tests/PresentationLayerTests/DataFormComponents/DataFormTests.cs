using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DataFormComponents;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DataFormComponents
{
    public class DataFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<DataForm> _testLogger;
        private readonly ILogger<IDataPresenter<DriversDTO>> _dataPresenterLogger;

        private readonly DataForm _noMsgBoxDataForm;
        private DataForm? _testMsgDataForm;

        private readonly DataModel<DriversDTO> _dataModel;
        private DataPresenter<DriversDTO>? _dataPresenter;

        public DataFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<DataForm>(output);
            _dataPresenterLogger = SharedFunctions.CreateTestLogger<IDataPresenter<DriversDTO>>(output);
            _noMsgBoxDataForm = new(_testLogger, new NoMessageBox());
            _dataModel = new(fixture.DriversRepository);
        }

        [Fact]
        public void btnSubmit_Click_RaisesSubmitClickedEvent()
        {
            // Arrange
            bool eventRaised = false;
            void eventHandler(object? sender, SubmissionCompletedEventArgs args)
            {
                eventRaised = true;
            }
            _noMsgBoxDataForm.SubmitClicked += eventHandler;

            // Act
            _noMsgBoxDataForm.btnSubmit_Click(this, EventArgs.Empty);

            // Assert
            Assert.True(eventRaised);

            // Cleanup
            _noMsgBoxDataForm.SubmitClicked -= eventHandler;
        }

        [Fact]
        public void btnSubmit_Click_WithoutSubscribers_DoesNotThrowException()
        {
            // Arrange

            // Act
            Exception exception = Record.Exception(() => _noMsgBoxDataForm.btnSubmit_Click(null, EventArgs.Empty));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("Test", "Details", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)]
        [InlineData("Warning", "Alert", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)]
        [InlineData("Error", "Critical", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error)]
        [InlineData("Info", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Information)]
        [InlineData("Question", "Query", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)]
        [InlineData("Stop", "Halt", MessageBoxButtons.RetryCancel, MessageBoxIcon.Stop)]
        public void ShowMessageBox_ShouldShowCorrectDetails(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon)
        {
            // Arrange
            TestMessageBox testMsgBox = new();
            _testMsgDataForm = new(_testLogger, testMsgBox);

            // Act
            _testMsgDataForm.ShowMessageBox(text, caption, button, icon);

            // Assert
            Assert.True(testMsgBox.WasShowCalled, "Message box was not shown");
            Assert.Equal(text, testMsgBox.LastMessage);
            Assert.Equal(caption, testMsgBox.LastCaption);
            Assert.Equal(button, testMsgBox.LastButton);
            Assert.Equal(icon, testMsgBox.LastIcon);
        }

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
            _testMsgDataForm = new(_testLogger);

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            // Act
            Dictionary<string, object> values = _dataModel.MapToForm(mockDriver);
            _dataPresenter = new(_testMsgDataForm, _dataModel);
            _testMsgDataForm.InitializeEditing(values);

            foreach (Control control in _testMsgDataForm.Controls)
            {
                _testLogger.LogInformation("Control: {Control}", control);
            }

            // Assert
            Assert.Equal(DriverID.ToString(), ((TextBox)_testMsgDataForm.Controls.Find("txtDriverID", true)[0]).Text);
            Assert.Equal(Name, ((TextBox)_testMsgDataForm.Controls.Find("txtName", true)[0]).Text);
            Assert.Equal(Surname, ((TextBox)_testMsgDataForm.Controls.Find("txtSurname", true)[0]).Text);
            Assert.Equal(EmployeeNo, ((TextBox)_testMsgDataForm.Controls.Find("txtEmployeeNo", true)[0]).Text);
            Assert.Equal(LicenseType.ToString(), ((ComboBox)_testMsgDataForm.Controls.Find("cboLicenseType", true)[0]).SelectedItem);
            Assert.Equal(Availability.ToString(), ((ComboBox)_testMsgDataForm.Controls.Find("cboAvailability", true)[0]).SelectedItem);
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
            _testMsgDataForm = new(_testLogger);

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            Dictionary<string, object> values = _dataModel.MapToForm(mockDriver);
            _dataPresenter = new(_testMsgDataForm, _dataModel, _dataPresenterLogger);
            _testMsgDataForm.InitializeEditing(values);

            // Act
            _testMsgDataForm.ClearData();

            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtDriverID", true)[0]).Text);
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtName", true)[0]).Text);
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtSurname", true)[0]).Text);
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtEmployeeNo", true)[0]).Text);
            Assert.Equal(-1, ((ComboBox)_testMsgDataForm.Controls.Find("cboLicenseType", true)[0]).SelectedIndex);
            Assert.Equal(-1, ((ComboBox)_testMsgDataForm.Controls.Find("cboAvailability", true)[0]).SelectedIndex);
        }

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", LicenseType.Code8, false)]
        [InlineData(2, "Jane", "Smith", "EMP002", LicenseType.Code8, true)]
        [InlineData(3, "Jim", "Brown", "EMP003", LicenseType.Code10, false)]
        [InlineData(4, "Jake", "White", "EMP004", LicenseType.Code10, true)]
        [InlineData(5, "Jill", "Green", "EMP005", LicenseType.Code14, false)]
        [InlineData(6, "Jack", "Black", "EMP006", LicenseType.Code14, true)]
        public void ClearData_WithDefaultsOverridden_ClearsDataFormFieldsWithOverrides(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            // Arrange
            _testMsgDataForm = new(_testLogger);

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            Dictionary<string, object> values = _dataModel.MapToForm(mockDriver);
            _dataPresenter = new(_testMsgDataForm, _dataModel, _dataPresenterLogger);
            _testMsgDataForm.InitializeEditing(values);

            Dictionary<string, object?> defaultValues = new()
            {
            { "DriverID", null },
            { "Name", null },
            { "Surname", null },
            { "EmployeeNo", null },
            { "LicenseType", null },
            { "Availability", true }
            };

            // Act
            _testMsgDataForm.ClearData(defaultValues);

            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtDriverID", true)[0]).Text);
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtName", true)[0]).Text);
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtSurname", true)[0]).Text);
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtEmployeeNo", true)[0]).Text);
            Assert.Equal(-1, ((ComboBox)_testMsgDataForm.Controls.Find("cboLicenseType", true)[0]).SelectedIndex);
            Assert.Equal("True", ((ComboBox)_testMsgDataForm.Controls.Find("cboAvailability", true)[0]).SelectedItem);
        }

        [Fact]
        public void RenderControls_CreatesCorrectControls()
        {
            // Arrange
            _testMsgDataForm = new(_testLogger);

            Dictionary<string, (Label Label, Control Control)> controlsLayout = new()
            {
            { "DriverID", (new Label { Text = "DriverID" }, new TextBox { Name = "txtDriverID" }) },
            { "Name", (new Label { Text = "Name" }, new TextBox { Name = "txtName" }) },
            { "Surname", (new Label { Text = "Surname" }, new TextBox { Name = "txtSurname" }) },
            { "EmployeeNo", (new Label { Text = "EmployeeNo" }, new TextBox { Name = "txtEmployeeNo" }) },
            { "LicenseType", (new Label { Text = "LicenseType" },new ComboBox{Name = "cboLicenseType",Items = { LicenseType.Code8, LicenseType.Code10, LicenseType.Code14 }})},
            { "Availability", (new Label { Text = "Availability" }, new ComboBox{Name = "cboAvailability",Items = { false, true }})}
            };

            // Act
            _testMsgDataForm.RenderControls(controlsLayout);

            // Assert
            Assert.NotNull(_testMsgDataForm.Controls.Find("txtDriverID", true)[0] as TextBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("txtName", true)[0] as TextBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("txtSurname", true)[0] as TextBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("txtEmployeeNo", true)[0] as TextBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("cboLicenseType", true)[0] as ComboBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("cboAvailability", true)[0] as ComboBox);
        }

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", LicenseType.Code8, false)]
        [InlineData(2, "Jane", "Smith", "EMP002", LicenseType.Code8, true)]
        [InlineData(3, "Jim", "Brown", "EMP003", LicenseType.Code10, false)]
        [InlineData(4, "Jake", "White", "EMP004", LicenseType.Code10, true)]
        [InlineData(5, "Jill", "Green", "EMP005", LicenseType.Code14, false)]
        [InlineData(6, "Jack", "Black", "EMP006", LicenseType.Code14, true)]
        public void GetControls_GetsDynamicControls(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            // Arrange
            _testMsgDataForm = new(_testLogger);

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            Dictionary<string, object> values = _dataModel.MapToForm(mockDriver);
            _dataPresenter = new(_testMsgDataForm, _dataModel, _dataPresenterLogger);
            _testMsgDataForm.InitializeEditing(values);

            // Act
            Dictionary<string, Control> _dynamicControls = _testMsgDataForm.GetControls();

            // Assert
            Assert.Equal(DriverID.ToString(), _dynamicControls["DriverID"].Text);
            Assert.Equal(Name, _dynamicControls["Name"].Text);
            Assert.Equal(Surname, _dynamicControls["Surname"].Text);
            Assert.Equal(EmployeeNo, _dynamicControls["EmployeeNo"].Text);
            Assert.Equal(LicenseType.ToString(), _dynamicControls["LicenseType"].Text);
            Assert.Equal(Availability.ToString(), _dynamicControls["Availability"].Text);
        }
    }
}

