using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests
{
    public class GenericDataFormTests
    {
        private readonly ILogger<GenericDataFormTemplate> _testLogger;
        private readonly GenericDataFormTemplate _noMsgBoxDataForm;
        private GenericDataFormTemplate? _testMsgDataForm;

        public GenericDataFormTests(ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<GenericDataFormTemplate>(output);
            _noMsgBoxDataForm = new(typeof(DriversDTO), TableConfigs.Drivers, _testLogger, new NoMessageBox());
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
            _testMsgDataForm = new(typeof(DriversDTO), TableConfigs.Drivers, _testLogger, testMsgBox);

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
            _testMsgDataForm = new(typeof(DriversDTO), TableConfigs.Drivers, _testLogger);

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            // Act
            _testMsgDataForm.InitializeEditing(mockDriver);

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
            _testMsgDataForm = new(typeof(DriversDTO), TableConfigs.Drivers, _testLogger);

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            _testMsgDataForm.InitializeEditing(mockDriver);

            // Act
            _testMsgDataForm.ClearData();

            // Assert
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtDriverID", true)[0]).Text);
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtName", true)[0]).Text);
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtSurname", true)[0]).Text);
            Assert.Equal("", ((TextBox)_testMsgDataForm.Controls.Find("txtEmployeeNo", true)[0]).Text);
            Assert.Equal(-1, ((ComboBox)_testMsgDataForm.Controls.Find("cboLicenseType", true)[0]).SelectedIndex);
            Assert.Equal("True", ((ComboBox)_testMsgDataForm.Controls.Find("cboAvailability", true)[0]).SelectedItem);
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
            _testMsgDataForm = new(typeof(DriversDTO), TableConfigs.Drivers, _testLogger);

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            _testMsgDataForm.InitializeEditing(mockDriver);

            // Act
            DriversDTO driver = (DriversDTO)_testMsgDataForm.GetData();

            // Assert
            Assert.Equal(DriverID, driver.DriverID);
            Assert.Equal(Name, driver.Name);
            Assert.Equal(Surname, driver.Surname);
            Assert.Equal(EmployeeNo, driver.EmployeeNo);
            Assert.Equal(LicenseType, driver.LicenseType);
            Assert.Equal(Availability, driver.Availability);
        }

        [Fact]
        public void GenerateDynamicFields_CreatesCorrectControls()
        {
            // Arrange
            _testMsgDataForm = new(typeof(DriversDTO), TableConfigs.Drivers, _testLogger);

            // Act
            // GenerateDynamicFields called in constructor

            // Assert
            Assert.NotNull(_testMsgDataForm.Controls.Find("txtDriverID", true)[0] as TextBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("txtName", true)[0] as TextBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("txtSurname", true)[0] as TextBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("txtEmployeeNo", true)[0] as TextBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("cboLicenseType", true)[0] as ComboBox);
            Assert.NotNull(_testMsgDataForm.Controls.Find("cboAvailability", true)[0] as ComboBox);

            ComboBox licenseCombo = (ComboBox)_testMsgDataForm.Controls.Find("cboLicenseType", true)[0];
            Assert.Equal(Enum.GetNames(typeof(LicenseType)).Length, licenseCombo.Items.Count);
            ComboBox availCombo = (ComboBox)_testMsgDataForm.Controls.Find("cboAvailability", true)[0];
            Assert.Equal(["False", "True"], availCombo.Items.Cast<string>());
            Assert.Equal(1, availCombo.SelectedIndex);
        }
    }
}
