using System.Data;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly;
using Polly.Registry;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement.Models
{
    public class DriverManagementModelTestBase : IClassFixture<DatabaseFixture>
    {
        protected readonly ILogger<DriverManagementModel> _testLogger;
        protected readonly DriversDAO _driversDAO;
        protected readonly PaginationManager<DriversDTO> _paginationManager;
        protected DriverManagementModel? _driverManagementModel;
        protected readonly bool _shouldSkipTests;
        protected readonly string _connectionString;

        protected ResiliencePipelineProvider<string> _mockPipelineProvider;
        protected IConfiguration _mockConfiguration;
        protected InMemorySink? _memorySink;
        protected ILogger<DriverManagementModel>? _memoryLogger;
        protected RetryEventService _mockRetryEventService;
        public DriverManagementModelTestBase(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<DriverManagementModel>(output);
            _connectionString = fixture.ConnectionString;
            if (fixture.CanConnectToDatabase == false)
            {
                _shouldSkipTests = true;
            }

            _mockPipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
            _mockPipelineProvider.GetPipeline("sql-retry-pipeline").Returns(ResiliencePipeline.Empty);
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockRetryEventService = Substitute.For<RetryEventService>();

            ILogger<DriversDAO> driversDAOTestLogger = SharedFunctions.CreateTestLogger<DriversDAO>(output);
            _driversDAO = new DriversDAO(_mockPipelineProvider, _mockConfiguration, driversDAOTestLogger, _connectionString, _mockRetryEventService);

            _paginationManager = new(_driversDAO, null);
        }

        internal void InitializeMemorySinkLogger()
        {
            (ILogger<DriverManagementModel> MemoryLogger, InMemorySink MemorySink) = SharedFunctions.CreateMemorySinkLogger<DriverManagementModel>();
            _memoryLogger = MemoryLogger;
            _memorySink = MemorySink;
        }
    }

    public class DriverManagementModelTests(DatabaseFixture fixture, ITestOutputHelper output) : DriverManagementModelTestBase(fixture, output)
    {
        [Fact]
        public async Task InitializeAsync_InitializesAndLogs()
        {
            // Arrange
            PaginationManager<DriversDTO> paginationManager = new(_driversDAO, null);
            _driverManagementModel = new(_driversDAO, paginationManager, _testLogger);

            // Act
            await _driverManagementModel.InitializeAsync();

            // Assert
            Assert.Equal(1, paginationManager.CurrentPage);
            Assert.Equal(6, paginationManager.TotalPages);
        }

        [Fact]
        public async Task InitializeAsync_ThrowsInvalidOperationException_WhenInitFails()
        {
            // Arrange
            ResiliencePipelineProvider<string> mockPipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
            IConfiguration mockConfiguration = Substitute.For<IConfiguration>();
            DriversDAO mockDriversDAO = Substitute.For<DriversDAO>(mockPipelineProvider, mockConfiguration, null, "mockConnection", null);

            // Real PaginationManager will try to access mock function with no set behaviour, causing error
            PaginationManager<DriversDTO> paginationManager = new(mockDriversDAO, null);
            _driverManagementModel = new(mockDriversDAO, paginationManager, _testLogger);

            // Act
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _driverManagementModel.InitializeAsync());

            // Assert
            Assert.Equal("Fatal error during initialization. Pagination will not function.", exception.Message);
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Equal("PaginationManager Initialization failed", exception.InnerException.Message);
        }

        [Fact]
        public async Task OnPageChanged_EmitsPageChangedEvent()
        {
            // Arrange
            _driverManagementModel = new(_driversDAO, _paginationManager, _testLogger);
            bool eventRaised = false;

            void PageChangedHandler(object? sender, EventArgs args) => eventRaised = true;
            _driverManagementModel.PageChanged += PageChangedHandler;

            // Act
            await _driverManagementModel.OnPageChanged(1);

            // Assert
            Assert.True(eventRaised);

            // Cleanup
            _driverManagementModel.PageChanged -= PageChangedHandler;
        }

        [Fact]
        public async Task OnPageChanged_SetsDgvTableToExpectedData_WhenPageFound()
        {
            // Arrange
            _driverManagementModel = new(_driversDAO, _paginationManager, _testLogger);
            await _driverManagementModel.InitializeAsync();

            // Act
            await _driverManagementModel.OnPageChanged(5);

            // Assert
            DataRow firstRow = _driverManagementModel.DgvTable.Rows[0];
            Assert.Equal("Michael", firstRow["Name"]);
            Assert.Equal("Taylor", firstRow["Surname"]);
            Assert.Equal("EMP2986", firstRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType.Code14, firstRow["LicenseType"]);
            Assert.Equal(true, firstRow["Availability"]);
        }

        [Fact]
        public async Task OnPageChanged_SetsDgvTableToEmpty_WhenGetDriversAtPageAsyncReturnsNull()
        {
            // Arrange
            _driverManagementModel = new(_driversDAO, _paginationManager, _testLogger);
            await _driverManagementModel.InitializeAsync();

            // Act
            await _driverManagementModel.OnPageChanged(999);

            // Assert
            Assert.Empty(_driverManagementModel.DgvTable.Rows);
        }

        [Fact]
        public void GetDriverFromRow_ReturnsCorrectDTO()
        {
            // Arrange
            _driverManagementModel = new(_driversDAO, _paginationManager, _testLogger);

            DataGridView dgv = new();
            dgv.Columns.Add(DriverColumns.DriverID, "DriverID");
            dgv.Columns.Add(DriverColumns.Name, "Name");
            dgv.Columns.Add(DriverColumns.Surname, "Surname");
            dgv.Columns.Add(DriverColumns.EmployeeNo, "EmployeeNo");
            dgv.Columns.Add(DriverColumns.LicenseType, "LicenseType");
            dgv.Columns.Add(DriverColumns.Availability, "Availability");

            dgv.Rows.Add(101, "John", "Doe", "EMP001", 1, true);
            dgv.Rows.Add(102, "Jane", "Smith", "EMP002", 2, false);

            DataGridViewRow selectedRow = dgv.Rows[1];

            // Act
            DriversDTO selectedDriver = _driverManagementModel.GetDriverFromRow(selectedRow);

            // Assert
            Assert.Equal(102, selectedDriver.DriverID);
            Assert.Equal("Jane", selectedDriver.Name);
            Assert.Equal("Smith", selectedDriver.Surname);
            Assert.Equal("EMP002", selectedDriver.EmployeeNo);
            Assert.Equal(LicenseType.Code10, selectedDriver.LicenseType);
            Assert.False(selectedDriver.Availability);
        }

        [Fact]
        public async Task FetchAndBindDriversAtPage_FetchesAndBindsDrivers_OfTheSpecifiedPage()
        {
            // Arrange
            PaginationManager<DriversDTO> manager = new(_driversDAO, null);
            DriverManagementModel driverManagementModel = new(_driversDAO, manager, _testLogger);
            await driverManagementModel.InitializeAsync();
            await driverManagementModel.PaginationManager.GoToLastPage();

            // Act
            await driverManagementModel.FetchAndBindDriversAtPage();

            // Assert
            DataTable result = driverManagementModel.DgvTable;
            Assert.NotNull(result);
            Assert.True(result.Rows.Count > 0, "No drivers were fetched from the database.");

            DataRow lastRow = result.Rows[result.Rows.Count - 1];
            Assert.Equal("Lucas", lastRow["Name"]);
            Assert.Equal("Miller", lastRow["Surname"]);
            Assert.Equal("EMP1234", lastRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType.Code8, lastRow["LicenseType"]);
            Assert.Equal(false, lastRow["Availability"]);
        }
    }

    [Collection("Sequential")]
    public class SequentialDriverManagementModelTests(DatabaseFixture fixture, ITestOutputHelper output) : DriverManagementModelTestBase(fixture, output)
    {
        [SkippableTheory]
        [InlineData("Olivia", "Clark", "EMP106", LicenseType.Code8, true)]
        [InlineData("Liam", "White", "EMP107", LicenseType.Code8, false)]
        [InlineData("Sophia", "Harris", "EMP108", LicenseType.Code10, true)]
        [InlineData("Noah", "Martin", "EMP109", LicenseType.Code10, false)]
        [InlineData("Ava", "Thompson", "EMP1010", LicenseType.Code14, true)]
        [InlineData("Mason", "Baker", "EMP1011", LicenseType.Code14, false)]
        public async Task AddDriverAsync_AddsDriverToDgvTableAndDatabase(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _driverManagementModel = new(_driversDAO, _paginationManager, _testLogger);
            await _driverManagementModel.InitializeAsync();

            DriversDTO mockDriver = new(
            DriverID: 999, // DriverID auto-increments server-side on insert. 999 is a placeholder 
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            // Act
            await _driverManagementModel.AddDriverAsync(mockDriver);

            // Assert
            DataRow? dgvTableLastRow = _driverManagementModel.DgvTable.Rows.Count > 0
            ? _driverManagementModel.DgvTable.Rows[_driverManagementModel.DgvTable.Rows.Count - 1]
            : null;

            Assert.NotNull(dgvTableLastRow);

            Assert.Equal(Name, dgvTableLastRow["Name"]);
            Assert.Equal(Surname, dgvTableLastRow["Surname"]);
            Assert.Equal(EmployeeNo, dgvTableLastRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, dgvTableLastRow["LicenseType"]);
            Assert.Equal(Availability, dgvTableLastRow["Availability"]);

            DataTable resultingDriver = await _driversDAO.GetRecordByPKAsync(106);
            DataRow dbFirstRow = resultingDriver.Rows[0];
            Assert.Equal(Name, dbFirstRow["Name"]);
            Assert.Equal(Surname, dbFirstRow["Surname"]);
            Assert.Equal(EmployeeNo, dbFirstRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, dbFirstRow["LicenseType"]);
            Assert.Equal(Availability, dbFirstRow["Availability"]);

            // Clean up
            await _driversDAO.DeleteRecordAsync(106);
            await _driversDAO.ReseedTable("Drivers", 105);
        }

        [SkippableFact]
        public async Task UpdateDriverAsync_LogsWarning_WhenDriverDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int DriverID = 999;

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: "Test",
            Surname: "Update",
            EmployeeNo: "EMP106",
            LicenseType: LicenseType.Code14,
            Availability: false
            );

            string message = $"Driver with ID {DriverID} was not found for update.";
            InitializeMemorySinkLogger();
            _driverManagementModel = new(_driversDAO, _paginationManager, _memoryLogger);
            await _driverManagementModel.InitializeAsync();

            // Act
            await _driverManagementModel.UpdateDriverAsync(mockDriver);

            // Assert

           SharedFunctions.AssertLogEventContainsMessage(_memorySink, LogEventLevel.Warning, message);
        }

        [SkippableTheory]
        [InlineData("Olivia", "Clark", "EMP106", LicenseType.Code8, true)]
        [InlineData("Liam", "White", "EMP107", LicenseType.Code8, false)]
        [InlineData("Sophia", "Harris", "EMP108", LicenseType.Code10, true)]
        [InlineData("Noah", "Martin", "EMP109", LicenseType.Code10, false)]
        [InlineData("Ava", "Thompson", "EMP1010", LicenseType.Code14, true)]
        [InlineData("Mason", "Baker", "EMP1011", LicenseType.Code14, false)]
        public async Task UpdateDriverAsync_UpdatesDetailsCorrectly_WhenDriverExists(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _driverManagementModel = new(_driversDAO, _paginationManager, _testLogger);

            int DriverID = 105;

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            await _driverManagementModel.OnPageChanged(6);  // Need an existing datatable to update

            // Act
            await _driverManagementModel.UpdateDriverAsync(mockDriver);

            // Assert
            DataRow? dgvTableLastRow = _driverManagementModel.DgvTable.Rows.Count > 0
           ? _driverManagementModel.DgvTable.Rows[_driverManagementModel.DgvTable.Rows.Count - 1]
           : null;

            Assert.NotNull(dgvTableLastRow);

            Assert.Equal(Name, dgvTableLastRow["Name"]);
            Assert.Equal(Surname, dgvTableLastRow["Surname"]);
            Assert.Equal(EmployeeNo, dgvTableLastRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, dgvTableLastRow["LicenseType"]);
            Assert.Equal(Availability, dgvTableLastRow["Availability"]);

            DataTable resultingDriver = await _driversDAO.GetRecordByPKAsync(105);
            DataRow dbFirstRow = resultingDriver.Rows[0];
            Assert.Equal(Name, dbFirstRow["Name"]);
            Assert.Equal(Surname, dbFirstRow["Surname"]);
            Assert.Equal(EmployeeNo, dbFirstRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, dbFirstRow["LicenseType"]);
            Assert.Equal(Availability, dbFirstRow["Availability"]);

            // Clean up
            DriversDTO Driver105 = new(
              DriverID: 105, // DriverID auto-increments server-side on insert. 999 is a placeholder 
              Name: "Lucas",
              Surname: "Miller",
              EmployeeNo: "EMP1234",
              LicenseType: LicenseType.Code8,
              Availability: false
              );
            await _driversDAO.UpdateRecordAsync(Driver105);
        }

        [SkippableFact]
        public async Task DeleteDriverAsync_LogsWarning_WhenDriverDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int DriverID = 999;

            string message = $"Driver with ID {DriverID} was not found for deletion.";
            InitializeMemorySinkLogger();
            _driverManagementModel = new(_driversDAO, _paginationManager, _memoryLogger);
            await _driverManagementModel.InitializeAsync();

            await _driverManagementModel.OnPageChanged(6);  // Need an existing datatable to delete from

            // Act
            await _driverManagementModel.DeleteDriverAsync(999);

            // Assert
            SharedFunctions.AssertLogEventContainsMessage(_memorySink, LogEventLevel.Warning, message);
        }

        [SkippableFact]
        public async Task DeleteDriverAsync_DeletesCorrectDriver()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _driverManagementModel = new(_driversDAO, _paginationManager, _testLogger);

            int DriverID = 105;
            await _driverManagementModel.OnPageChanged(6);  // Need an existing datatable to update

            // Act
            await _driverManagementModel.DeleteDriverAsync(DriverID);

            // Assert
            bool driverExists = _driverManagementModel.DgvTable.AsEnumerable()
            .Any(row => row.Field<int>("DriverID") == 105);
            Assert.False(driverExists, "DriverID 105 should not exist in the DataTable after deletion.");

            DataTable result = await _driversDAO.GetRecordByPKAsync(DriverID);
            Assert.NotNull(result);
            Assert.Empty(result.Rows);

            // Clean up
            DriversDTO Driver105 = new(
              DriverID: 105, // DriverID auto-increments server-side on insert. 105 is a placeholder 
              Name: "Lucas",
              Surname: "Miller",
              EmployeeNo: "EMP1234",
              LicenseType: LicenseType.Code8,
              Availability: false
              );
            await _driversDAO.ReseedTable("Drivers", 104);
            await _driversDAO.InsertRecordAsync(Driver105);
        }
    }


}
