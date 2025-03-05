using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement
{
    public class DriverManagementFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly DriversDAO _driversDAO;
        private readonly string _connectionString;

        private readonly DriverDataForm _dataform;
        private readonly DriverManagementForm _managementForm;

        public DriverManagementFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _driversDAO = fixture.DriversDAO;
            _managementForm = fixture.DriverManagementForm;
            _connectionString = fixture.ConnectionString;
            //  _dataform = new DriverDataForm(_driversDAO);
            _output = output;
        }

        // Incomplete
        //[Theory]
        //[InlineData("Test", "Insert", "EMP010", LicenseType.Code14, false)]
        //public void btnAdd_Click_ShouldInsertDataToDB(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        //{
        //    using (SqlConnection connection = new(_connectionString))
        //    {
        //        connection.Open();

        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                var mockDriver = new DriversDTO(
        //                    DriverID: 900, // DriverID auto-increments server-side on insert. 
        //                    Name: Name,
        //                    Surname: Surname,
        //                    EmployeeNo: EmployeeNo,
        //                    LicenseType: LicenseType,
        //                    Availability: Availability
        //                );

        //                // Initialize form and attach event handler
        //                _dataform.InitializeEditing(mockDriver);
        //                _dataform.SubmitClicked += _managementForm.DriverDataForm_SubmitClicked;

        //                _dataform.btnSubmit_Click(mockDriver, null);

        //                // Validate the insertion
        //                DataTable insertedDriver = _driversDAO.GetDriverByID(6); // Assumming test database always has 5 records
        //                DataRow driver = insertedDriver.Rows[0];

        //                _output.WriteLine($"Driver Info: ID = {driver["DriverID"]}, Name = {driver["Name"]}, Surname = {driver["Surname"]}, " +
        //                                    $"EmployeeNo = {driver["EmployeeNo"]}, LicenseType = {driver["LicenseType"]}, Availability = {driver["Availability"]}");

        //                Assert.Equal(mockDriver.DriverID, driver["DriverID"]);
        //                Assert.Equal(mockDriver.Name, driver["Name"]);
        //                Assert.Equal(mockDriver.Surname, driver["Surname"]);
        //                Assert.Equal(mockDriver.EmployeeNo, driver["EmployeeNo"]);
        //                Assert.Equal(mockDriver.LicenseType, driver["LicenseType"]);
        //                Assert.Equal(mockDriver.Availability, driver["Availability"]);

        //                transaction.Rollback();
        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                throw new Exception("Test failed, rolling back transaction.", ex);
        //            }
        //        }
        //    }
        //}
    }
}
