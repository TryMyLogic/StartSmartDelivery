using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using NSubstitute;
using StartSmartDeliveryForm.DataLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement.Views
{
    public class DriverManagementFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<DriverManagementForm> _testLogger;
        private  DriverManagementForm? _managementForm;

        public DriverManagementFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<DriverManagementForm>(output);
        }

        [Fact]
        public void UpdatePaginationDisplay_UpdatesDisplayCorrectly()
        {
            // Arrange
            _managementForm = new(_testLogger);

            // Act
            _managementForm.UpdatePaginationDisplay(5, 10);

            // Assert
            Assert.Equal($"{5}", _managementForm.StartPageText);
            Assert.Equal($"/{10}", _managementForm.EndPageText);
        }

        [Fact]
        public void SetDataGridViewColumns_SetsCorrectDisplayIndexesAndHidesDriverID()
        {
            // Arrange
            _managementForm = new(_testLogger);

            _managementForm.DgvMain.Columns.Add(DriverColumns.Name, "Name");
            _managementForm.DgvMain.Columns.Add(DriverColumns.Surname, "Surname");
            _managementForm.DgvMain.Columns.Add(DriverColumns.EmployeeNo, "Employee No");
            _managementForm.DgvMain.Columns.Add(DriverColumns.LicenseType, "License Type");
            _managementForm.DgvMain.Columns.Add(DriverColumns.Availability, "Availability");
            _managementForm.DgvMain.Columns.Add(DriverColumns.Edit, "Edit");
            _managementForm.DgvMain.Columns.Add(DriverColumns.Delete, "Delete");
            _managementForm.DgvMain.Columns.Add(DriverColumns.DriverID, "Driver ID");

            // Act
            _managementForm.SetDataGridViewColumns();

            // Assert
            Assert.Equal(0, _managementForm.DgvMain.Columns[DriverColumns.Name].DisplayIndex);
            Assert.Equal(1, _managementForm.DgvMain.Columns[DriverColumns.Surname].DisplayIndex);
            Assert.Equal(2, _managementForm.DgvMain.Columns[DriverColumns.EmployeeNo].DisplayIndex);
            Assert.Equal(3, _managementForm.DgvMain.Columns[DriverColumns.LicenseType].DisplayIndex);
            Assert.Equal(4, _managementForm.DgvMain.Columns[DriverColumns.Availability].DisplayIndex);
            Assert.Equal(5, _managementForm.DgvMain.Columns[DriverColumns.Edit].DisplayIndex);
            Assert.Equal(6, _managementForm.DgvMain.Columns[DriverColumns.Delete].DisplayIndex);
            Assert.False(_managementForm.DgvMain.Columns[DriverColumns.DriverID].Visible);
        }
    }
}
