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

namespace StartSmartDeliveryForm.Tests
{
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

        [Theory]
        [InlineData("Test", "Insert", "EMP010", LicenseType.Code14, false)]
        public void InsertDriver_ShouldInsertDataToDB(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            using (SqlConnection connection = new(_connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Arrange
                        var mockDriver = new DriversDTO(
                            DriverID: 6, // DriverID auto-increments server-side on insert. 
                            Name: Name,
                            Surname: Surname,
                            EmployeeNo: EmployeeNo,
                            LicenseType: LicenseType,
                            Availability: Availability
                        );

                        // Act
                        _driversDAO.InsertDriver(mockDriver);

                        // Assert
                        DataTable insertedDriver = _driversDAO.GetDriverByID(6); // Assumming test database always has 5 records
                        DataRow driver = insertedDriver.Rows[0];

                        Assert.Equal(mockDriver.DriverID, driver["DriverID"]);
                        Assert.Equal(mockDriver.Name, driver["Name"]);
                        Assert.Equal(mockDriver.Surname, driver["Surname"]);
                        Assert.Equal(mockDriver.EmployeeNo, driver["EmployeeNo"]);
                        Assert.Equal((int)mockDriver.LicenseType, driver["LicenseType"]);
                        Assert.Equal(mockDriver.Availability, driver["Availability"]);

                        _output.WriteLine($"Driver Info: ID = {driver["DriverID"]}, Name = {driver["Name"]}, Surname = {driver["Surname"]}, " +
                                          $"EmployeeNo = {driver["EmployeeNo"]}, LicenseType = {driver["LicenseType"]}, Availability = {driver["Availability"]}");

                        transaction.Rollback(); // Ensures database data remains constant
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Test failed, rolling back transaction.", ex);
                    }
                }
            }
        }
    }
}
