using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.SharedLayer.Enums;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.DataLayerTests
{

    [Collection("Sequential")]
    public class DriversDAOTests : IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly DriversDAO _driversDAO;
        private readonly DriverDataForm _dataform;
        private readonly DriverManagementForm _managementForm;
        private readonly string _connectionString;

        public DriversDAOTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _driversDAO = fixture.DriversDAO;
            _managementForm = fixture.DriverManagementForm;
            _connectionString = fixture.ConnectionString;
            _dataform = new DriverDataForm(_driversDAO);
            _output = output;
        }

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", 1, true)]
        [InlineData(2, "Jane", "Smith", "EMP002", 2, false)]
        public void GetDriverByID_ReturnsCorrectDriver_WhenDriverExists(int DriverID, string Name, string Surname, string EmployeeNo, int LicenseType, bool Availability)
        {
            // Arrange

            // Act
            DataTable result = _driversDAO.GetDriverByID(DriverID);

            // Assert
            DataRow firstRow = result.Rows[0];

            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal(DriverID, firstRow["DriverID"]);
            Assert.Equal(Name, firstRow["Name"]);
            Assert.Equal(Surname, firstRow["Surname"]);
            Assert.Equal(EmployeeNo, firstRow["EmployeeNo"]);
            Assert.Equal(LicenseType, firstRow["LicenseType"]);
            Assert.Equal(Availability, firstRow["Availability"]);

            _output.WriteLine($"Driver: {firstRow["Name"]} ({Name}), {firstRow["Surname"]} ({Surname}), " +
                     $"ID: {firstRow["DriverID"]} ({DriverID}), EmployeeNo: {firstRow["EmployeeNo"]} ({EmployeeNo}), " +
                     $"LicenseType: {firstRow["LicenseType"]} ({LicenseType}), Availability: {firstRow["Availability"]} ({Availability})");
        }

        [Fact]
        public void GetDriverByID_ReturnsEmptyTable_WhenDriverDoesNotExist()
        {
            // Arrange
            int nonExistentID = 9999;

            // Act
            DataTable result = _driversDAO.GetDriverByID(nonExistentID);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Rows);
        }

    }
}
