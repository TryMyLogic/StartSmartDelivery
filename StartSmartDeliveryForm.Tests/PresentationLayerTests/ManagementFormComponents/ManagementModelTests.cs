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
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.DataLayer.Repositories;
using StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.ManagementFormComponents
{
    public abstract class ManagementModelTestsBase : IClassFixture<DatabaseFixture>
    {
        protected readonly ILogger<ManagementModel<DriversDTO>> _testLogger;
        protected readonly Repository<DriversDTO> _repository;
        protected readonly PaginationManager<DriversDTO> _paginationManager;
        protected ManagementModel<DriversDTO>? _managementModel;
        protected readonly bool _shouldSkipTests;
        protected readonly string _connectionString;

        protected ResiliencePipelineProvider<string> _mockPipelineProvider;
        protected IConfiguration _mockConfiguration;
        protected InMemorySink? _memorySink;
        protected ILogger<ManagementModel<DriversDTO>>? _memoryLogger;
        protected RetryEventService _mockRetryEventService;

        protected ManagementModelTestsBase(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<ManagementModel<DriversDTO>>(output);
            _connectionString = fixture.ConnectionString;
            _shouldSkipTests = !fixture.CanConnectToDatabase;

            _mockPipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
            _mockPipelineProvider.GetPipeline("sql-retry-pipeline").Returns(ResiliencePipeline.Empty);
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockRetryEventService = Substitute.For<RetryEventService>();

            ILogger<Repository<DriversDTO>> repositoryLogger = SharedFunctions.CreateTestLogger<Repository<DriversDTO>>(output);
            _repository = new Repository<DriversDTO>(_mockPipelineProvider, _mockConfiguration, repositoryLogger, _connectionString, _mockRetryEventService);

            _paginationManager = new(_repository, null);
        }

        protected void InitializeMemorySinkLogger()
        {
            (ILogger<ManagementModel<DriversDTO>> MemoryLogger, InMemorySink MemorySink) = SharedFunctions.CreateMemorySinkLogger<ManagementModel<DriversDTO>>();
            _memoryLogger = MemoryLogger;
            _memorySink = MemorySink;
        }
    }

    public class ManagementModelTests(DatabaseFixture fixture, ITestOutputHelper output) : ManagementModelTestsBase(fixture, output)
    {
        [Theory]
        // Valid cases for filtering
        [InlineData("DriverID", "1", 1)]

        // Invalid cases (no matches)
        [InlineData("DriverID", "5", 0)]

        // Null input
        [InlineData("DriverID", null, 0)]
        public void ApplyFilter_ShouldFilterDriverID(string selectedOption, string? searchTerm, int expectedRowCount)
        {
            // Arrange
            DataTable dataTable = new();
            dataTable.Columns.Add("DriverID", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("LicenseType", typeof(int));
            dataTable.Columns.Add("Availability", typeof(bool));

            dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);
            dataTable.Rows.Add(2, "Jane", (int)LicenseType.Code10, false);
            dataTable.Rows.Add(3, "John", (int)LicenseType.Code14, true);
            dataTable.Rows.Add(4, "Doe", (int)LicenseType.Code8, false);

            IRepository<DriversDTO> mockRepository = Substitute.For<IRepository<DriversDTO>>();
            _managementModel = new(mockRepository, _paginationManager, _testLogger);
            SearchRequestEventArgs eventArgsMock = Substitute.For<SearchRequestEventArgs>(dataTable, selectedOption, searchTerm, false);

            // Act
            _managementModel.ApplyFilter(null, eventArgsMock);
            DataTable filteredTable = _managementModel.DgvTable;
            List<DataRow> filteredRows = [.. filteredTable.AsEnumerable().Where(row => row.RowState != DataRowState.Deleted)];

            // Assert
            Assert.Equal(expectedRowCount, filteredRows.Count);
            if (expectedRowCount > 0)
            {
                Assert.Single(filteredRows); // Each DriverID is unique
            }
        }

        [Theory]
        // Valid cases for filtering
        [InlineData("Name", "John", false, 2)]

        // Invalid cases (no matches)
        [InlineData("Name", "Tom", false, 0)]

        // Case sensitive string matching
        [InlineData("Name", "JoHn", true, 0)]

        // Null input
        [InlineData("Name", "null", false, 1)]
        public void ApplyFilter_ShouldFilterName(string selectedOption, string searchTerm, bool isCaseSensitive, int expectedRowCount)
        {
            // Arrange
            DataTable dataTable = new();
            dataTable.Columns.Add("DriverID", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("LicenseType", typeof(int));
            dataTable.Columns.Add("Availability", typeof(bool));

            dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);
            dataTable.Rows.Add(2, "Jane", (int)LicenseType.Code10, false);
            dataTable.Rows.Add(3, "John", (int)LicenseType.Code14, true);
            dataTable.Rows.Add(4, "null", (int)LicenseType.Code8, false);

            IRepository<DriversDTO> mockRepository = Substitute.For<IRepository<DriversDTO>>();
            _managementModel = new(mockRepository, _paginationManager, _testLogger);
            SearchRequestEventArgs eventArgsMock = Substitute.For<SearchRequestEventArgs>(dataTable, selectedOption, searchTerm, isCaseSensitive);

            // Act
            _managementModel.ApplyFilter(null, eventArgsMock);
            DataTable filteredTable = _managementModel.DgvTable;
            List<DataRow> filteredRows = [.. filteredTable.AsEnumerable().Where(row => row.RowState != DataRowState.Deleted)];

            // Assert
            Assert.Equal(expectedRowCount, filteredRows.Count);
            if (expectedRowCount > 0 && searchTerm != "null")
            {
                Assert.Contains("John", filteredRows.Select(row => row["Name"].ToString()));
            }
            else if (searchTerm == "null")
            {
                Assert.Contains("null", filteredRows.Select(row => row["Name"].ToString()));
            }
        }

        [Theory]
        // Valid cases for filtering
        [InlineData("LicenseType", "Code8", false, 1)]
        [InlineData("LicenseType", "1", false, 1)]

        // Invalid cases (no matches)
        [InlineData("LicenseType", "CoDe8", true, 0)]
        [InlineData("LicenseType", "4", false, 0)]

        // Null input
        [InlineData("LicenseType", null, false, 0)]
        public void ApplyFilter_ShouldFilterLicenseType(string selectedOption, string? searchTerm, bool isCaseSensitive, int expectedRowCount)
        {
            // Arrange
            DataTable dataTable = new();
            dataTable.Columns.Add("DriverID", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("LicenseType", typeof(int));
            dataTable.Columns.Add("Availability", typeof(bool));

            dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);

            IRepository<DriversDTO> mockRepository = Substitute.For<IRepository<DriversDTO>>();
            _managementModel = new(mockRepository, _paginationManager, _testLogger);
            SearchRequestEventArgs eventArgsMock = Substitute.For<SearchRequestEventArgs>(dataTable, selectedOption, searchTerm, isCaseSensitive);

            // Act
            _managementModel.ApplyFilter(null, eventArgsMock);
            DataTable filteredTable = _managementModel.DgvTable;
            List<DataRow> filteredRows = [.. filteredTable.AsEnumerable().Where(row => row.RowState != DataRowState.Deleted)];

            // Assert
            Assert.Equal(expectedRowCount, filteredRows.Count);
            if (expectedRowCount > 0)
            {
                Assert.Contains("1", filteredRows.Select(row => row["LicenseType"].ToString()));
            }
        }

        [Theory]
        // Valid cases for filtering
        [InlineData("Availability", "True", false, 1)]
        [InlineData("Availability", "1", false, 1)]
        [InlineData("Availability", "False", false, 1)]
        [InlineData("Availability", "0", false, 1)]

        //Case sensitivity (Shouldnt apply to bools)
        [InlineData("Availability", "TrUe", true, 1)]

        // Invalid cases (no matches)
        [InlineData("Availability", "Invalid", false, 0)]
        [InlineData("Availability", "2", false, 0)]

        // Null input
        [InlineData("Availability", null, false, 0)]
        public void ApplyFilter_ShouldFilterAvailability(string selectedOption, string? searchTerm, bool isCaseSensitive, int expectedRowCount)
        {
            // Arrange
            DataTable dataTable = new();
            dataTable.Columns.Add("DriverID", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("LicenseType", typeof(int));
            dataTable.Columns.Add("Availability", typeof(bool));

            dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);
            dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, false);

            IRepository<DriversDTO> mockRepository = Substitute.For<IRepository<DriversDTO>>();
            _managementModel = new(mockRepository, _paginationManager, _testLogger);
            SearchRequestEventArgs eventArgsMock = Substitute.For<SearchRequestEventArgs>(dataTable, selectedOption, searchTerm, isCaseSensitive);

            // Act
            _managementModel.ApplyFilter(null, eventArgsMock);
            DataTable filteredTable = _managementModel.DgvTable;
            List<DataRow> filteredRows = [.. filteredTable.AsEnumerable().Where(row => row.RowState != DataRowState.Deleted)];

            // Assert
            Assert.Equal(expectedRowCount, filteredRows.Count);
            if (expectedRowCount > 0 && searchTerm != null)
            {
                bool expectedBoolValue = searchTerm.Equals("True", StringComparison.OrdinalIgnoreCase) || searchTerm.Equals("1");
                Assert.True(filteredRows.All(row => row.Field<bool>("Availability") == expectedBoolValue));
            }
        }

        [Theory]
        // Valid cases for filtering (case in-sensitive)
        [InlineData("DriverID", "1", false, 1)]
        [InlineData("Name", "John", false, "John")]
        [InlineData("LicenseType", "Code8", false, (int)LicenseType.Code8)]
        [InlineData("Availability", "True", false, true)]

        // Invalid cases (no matches)
        [InlineData("DriverID", "", false, null)]
        [InlineData("Name", "", false, null)]
        [InlineData("LicenseType", "", false, null)]
        [InlineData("Availability", "", false, null)]

        // Boolean values as numbers and different cases
        //[InlineData("Availability", "True", false, true)] - Already tested
        [InlineData("Availability", "False", false, false)]
        [InlineData("Availability", "1", false, true)]
        [InlineData("Availability", "0", false, false)]

        // Case sensitive string matching
        [InlineData("Name", "JoHn", true, null)]

        // Case insensitive filtering
        [InlineData("Name", "john", false, "John")]

        // Invalid column test
        [InlineData("InvalidColumn", "Test", false, null)]

        // Special characters 
        [InlineData("Name", "O'Reilly", false, "O'Reilly")]

        // Null input test
        [InlineData("Name", null, false, null)] //searchTerm would never be null but testing anyways
        [InlineData("Name", "null", false, "null")] //For tables that allow null inputs

        public void FilterRows_ShouldReturnCorrectRows(string selectedOption, string? searchTerm, bool isCaseSensitive, object? expectedValue)
        {
            // Arrange
            DataTable dataTable = new();
            dataTable.Columns.Add("DriverID", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("LicenseType", typeof(int));
            dataTable.Columns.Add("Availability", typeof(bool));

            dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);
            dataTable.Rows.Add(2, "O'Reilly", (int)LicenseType.Code10, false);
            dataTable.Rows.Add(2, "null", (int)LicenseType.Code14, false);

            IRepository<DriversDTO> mockRepository = Substitute.For<IRepository<DriversDTO>>();
            _managementModel = new(mockRepository, _paginationManager, _testLogger);
            SearchRequestEventArgs eventArgsMock = Substitute.For<SearchRequestEventArgs>(dataTable, selectedOption, searchTerm, isCaseSensitive);

            // Act
            _managementModel.ApplyFilter(null, eventArgsMock);
            DataTable filteredTable = _managementModel.DgvTable;
            List<DataRow> filteredRows = [.. filteredTable.AsEnumerable().Where(row => row.RowState != DataRowState.Deleted)];

            // Assert
            if (expectedValue == null)
            {
                if (searchTerm == "null")
                {
                    Assert.Equal(expectedValue, filteredRows[0][selectedOption]);
                }
                // Expect no matches, so filteredRows should be empty
                Assert.Empty(filteredRows);
            }
            else if (searchTerm != null && selectedOption == "Availability" &&
                    (searchTerm.Equals("False", StringComparison.OrdinalIgnoreCase) || searchTerm.Equals("0")))
            {
                Assert.Equal(2, filteredRows.Count);
            }
            else
            {
                Assert.Single(filteredRows);
                Assert.Equal(expectedValue, filteredRows[0][selectedOption]);
            }
        }

        [Fact]
        public async Task InitializeAsync_InitializesAndLogs()
        {
            // Arrange
            PaginationManager<DriversDTO> paginationManager = new(_repository, null);
            _managementModel = new(_repository, paginationManager, _testLogger);

            // Act
            await _managementModel.InitializeAsync();

            // Assert
            Assert.Equal(1, _managementModel.PaginationManager.CurrentPage);
            Assert.Equal(6, _managementModel.PaginationManager.TotalPages); // Adjust based on your DB data
        }

        [Fact]
        public async Task InitializeAsync_ThrowsInvalidOperationException_WhenInitFails()
        {
            // Arrange
            IRepository<DriversDTO> _repositoryMock = Substitute.For<IRepository<DriversDTO>>();
            _repositoryMock.GetRecordCountAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new InvalidOperationException("PaginationManager Initialization failed")));

            // Real PaginationManager will try to access mock function with no set behaviour, causing error
            PaginationManager<DriversDTO> paginationManager = new(_repositoryMock);
            _managementModel = new(_repository, paginationManager, _testLogger);

            // Act
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _managementModel.InitializeAsync());

            // Assert
            Assert.Equal("Fatal error during initialization. Pagination will not function.", exception.Message);
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Equal("PaginationManager Initialization failed", exception.InnerException.Message);
        }

        [Fact]
        public async Task OnPageChanged_EmitsPageChangedEvent()
        {
            // Arrange
            _managementModel = new(_repository, _paginationManager, _testLogger);

            bool eventRaised = false;
            void PageChangedHandler(object? sender, EventArgs args) => eventRaised = true;
            _managementModel.PageChanged += PageChangedHandler;

            // Act
            await _managementModel.OnPageChanged(1);

            // Assert
            Assert.True(eventRaised);
            _managementModel.PageChanged -= PageChangedHandler;
        }

        [Fact]
        public async Task OnPageChanged_SetsDgvTableToExpectedData_WhenPageFound()
        {
            // Arrange
            _managementModel = new(_repository, _paginationManager, _testLogger);
            await _managementModel.InitializeAsync();

            // Act
            await _managementModel.OnPageChanged(5);

            // Assert
            DataRow firstRow = _managementModel.DgvTable.Rows[0];
            Assert.Equal("Michael", firstRow["Name"]);
            Assert.Equal("Taylor", firstRow["Surname"]);
            Assert.Equal("EMP2986", firstRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType.Code14, firstRow["LicenseType"]);
            Assert.Equal(true, firstRow["Availability"]);
        }

        [Fact]
        public async Task OnPageChanged_SetsDgvTableToEmpty_WhenGetRecordsAtPageAsyncReturnsNull()
        {
            // Arrange
            _managementModel = new(_repository, _paginationManager, _testLogger);
            await _managementModel.InitializeAsync();

            // Act
            await _managementModel.OnPageChanged(999);

            // Assert
            Assert.Empty(_managementModel.DgvTable.Rows);
        }

        [Fact]
        public void GetEntityFromRow_ReturnsCorrectDTO()
        {
            // Arrange
            _managementModel = new(_repository, _paginationManager, _testLogger);
            DataGridView dgv = new();
            dgv.Columns.Add("DriverID", "DriverID");
            dgv.Columns.Add("Name", "Name");
            dgv.Columns.Add("Surname", "Surname");
            dgv.Columns.Add("EmployeeNo", "EmployeeNo");
            dgv.Columns.Add("LicenseType", "LicenseType");
            dgv.Columns.Add("Availability", "Availability");

            dgv.Rows.Add(101, "John", "Doe", "EMP001", 1, true);
            dgv.Rows.Add(102, "Jane", "Smith", "EMP002", 2, false);

            DataGridViewRow selectedRow = dgv.Rows[1];

            // Act
            DriversDTO selectedDriver = _managementModel.GetEntityFromRow(selectedRow);

            // Assert
            Assert.Equal(102, selectedDriver.DriverID);
            Assert.Equal("Jane", selectedDriver.Name);
            Assert.Equal("Smith", selectedDriver.Surname);
            Assert.Equal("EMP002", selectedDriver.EmployeeNo);
            Assert.Equal(LicenseType.Code10, selectedDriver.LicenseType);
            Assert.False(selectedDriver.Availability);
        }
    }

    [Collection("Sequential")]
    public class SequentialManagementModelTests(DatabaseFixture fixture, ITestOutputHelper output) : ManagementModelTestsBase(fixture, output)
    {
        [Fact]
        public async Task FetchAndBindRecordsAtPageAsync_FetchesAndBindsDrivers_OfTheSpecifiedPage()
        {
            // Arrange
            PaginationManager<DriversDTO> paginationManager = new(_repository);
            _managementModel = new(_repository, paginationManager, _testLogger);
            await _managementModel.InitializeAsync();
            await _managementModel.PaginationManager.GoToLastPageAsync();

            // Act
            await _managementModel.FetchAndBindRecordsAtPageAsync();

            // Assert
            DataTable result = _managementModel.DgvTable;
            Assert.NotNull(result);
            Assert.True(result.Rows.Count > 0, "No drivers were fetched from the database.");

            DataRow lastRow = result.Rows[result.Rows.Count - 1];
            Assert.Equal("Lucas", lastRow["Name"]);
            Assert.Equal("Miller", lastRow["Surname"]);
            Assert.Equal("EMP1234", lastRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType.Code8, lastRow["LicenseType"]);
            Assert.Equal(false, lastRow["Availability"]);
        }

        [SkippableTheory]
        [InlineData("Olivia", "Clark", "EMP106", LicenseType.Code8, true)]
        public async Task AddRecordAsync_AddsDriverToDgvTableAndDatabase(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _managementModel = new(_repository, _paginationManager, _testLogger);
            await _managementModel.InitializeAsync();

            DriversDTO mockDriver = new(
            DriverID: 999, // DriverID auto-increments server-side on insert. 999 is a placeholder 
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            // Act
            await _managementModel.AddRecordAsync(mockDriver);

            // Assert
            DataRow? dgvTableLastRow = _managementModel.DgvTable.Rows.Count > 0
                     ? _managementModel.DgvTable.Rows[_managementModel.DgvTable.Rows.Count - 1]
                     : null;

            Assert.NotNull(dgvTableLastRow);

            Assert.Equal(Name, dgvTableLastRow["Name"]);
            Assert.Equal(Surname, dgvTableLastRow["Surname"]);
            Assert.Equal(EmployeeNo, dgvTableLastRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, dgvTableLastRow["LicenseType"]);
            Assert.Equal(Availability, dgvTableLastRow["Availability"]);

            DataTable resultingDriver = await _repository.GetRecordByPKAsync(106);
            DataRow dbFirstRow = resultingDriver.Rows[0];
            Assert.Equal(Name, dbFirstRow["Name"]);
            Assert.Equal(Surname, dbFirstRow["Surname"]);
            Assert.Equal(EmployeeNo, dbFirstRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, dbFirstRow["LicenseType"]);
            Assert.Equal(Availability, dbFirstRow["Availability"]);

            // Cleanup
            await _repository.DeleteRecordAsync(106);
            await _repository.ReseedTableAsync(105);
        }

        [SkippableFact]
        public async Task UpdateRecordAsync_LogsWarning_WhenDriverDoesNotExist()
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

            string message = $"\"Drivers\" with PK {DriverID} was not found for update";
            InitializeMemorySinkLogger();
            _managementModel = new(_repository, _paginationManager, _memoryLogger);
            await _managementModel.InitializeAsync();

            // Act
            await _managementModel.UpdateRecordAsync(mockDriver);

            // Assert
            SharedFunctions.AssertLogEventContainsMessage(_memorySink, LogEventLevel.Warning, message);
        }

        [SkippableTheory]
        [InlineData("Olivia", "Clark", "EMP106", LicenseType.Code8, true)]
        public async Task UpdateRecordAsync_UpdatesDetailsCorrectly_WhenDriverExists(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _managementModel = new(_repository, _paginationManager, _testLogger);

            int DriverID = 105;

            await _managementModel.OnPageChanged(6);

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            // Act
            await _managementModel.UpdateRecordAsync(mockDriver);

            // Assert
            DataRow? dgvTableLastRow = _managementModel.DgvTable.Rows.Count > 0
           ? _managementModel.DgvTable.Rows[_managementModel.DgvTable.Rows.Count - 1]
           : null;

            Assert.NotNull(dgvTableLastRow);

            Assert.Equal(Name, dgvTableLastRow["Name"]);
            Assert.Equal(Surname, dgvTableLastRow["Surname"]);
            Assert.Equal(EmployeeNo, dgvTableLastRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, dgvTableLastRow["LicenseType"]);
            Assert.Equal(Availability, dgvTableLastRow["Availability"]);

            DataTable resultingDriver = await _repository.GetRecordByPKAsync(105);
            DataRow dbFirstRow = resultingDriver.Rows[0];
            Assert.Equal(Name, dbFirstRow["Name"]);
            Assert.Equal(Surname, dbFirstRow["Surname"]);
            Assert.Equal(EmployeeNo, dbFirstRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, dbFirstRow["LicenseType"]);
            Assert.Equal(Availability, dbFirstRow["Availability"]);

            // Cleanup
            DriversDTO Driver105 = new(
            DriverID: 105, // DriverID auto-increments server-side on insert. 999 is a placeholder 
            Name: "Lucas",
            Surname: "Miller",
            EmployeeNo: "EMP1234",
            LicenseType: LicenseType.Code8,
            Availability: false
            );

            await _repository.UpdateRecordAsync(Driver105);
        }

        [SkippableFact]
        public async Task DeleteDriverAsync_LogsWarning_WhenDriverDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int DriverID = 999;

            string message = $"Row with PK {DriverID} not found in \"Drivers\" for deletion";
            InitializeMemorySinkLogger();
            _managementModel = new(_repository, _paginationManager, _memoryLogger);
            await _managementModel.InitializeAsync();
            await _managementModel.OnPageChanged(6);

            // Act
            await _managementModel.DeleteRecordAsync(999);

            // Assert
            SharedFunctions.AssertLogEventContainsMessage(_memorySink, LogEventLevel.Warning, message, _testLogger);
        }

        [SkippableFact]
        public async Task DeleteRecordAsync_DeletesCorrectDriver()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange

            _managementModel = new(_repository, _paginationManager, _testLogger);

            int DriverID = 105;
            await _managementModel.OnPageChanged(6); // Load page with DriverID 105

            // Act
            await _managementModel.DeleteRecordAsync(DriverID);

            // Assert
            bool driverExists = _managementModel.DgvTable.AsEnumerable().Any(row => row.Field<int>("DriverID") == DriverID);
            Assert.False(driverExists);

            DataTable result = await _repository.GetRecordByPKAsync(DriverID);
            Assert.Empty(result.Rows);

            // Cleanup
            DriversDTO Driver105 = new(105, "Lucas", "Miller", "EMP1234", LicenseType.Code8, false);
            await _repository.ReseedTableAsync(104);
            await _repository.InsertRecordAsync(Driver105);
        }
    }


}
