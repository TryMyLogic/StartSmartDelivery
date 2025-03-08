using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly;
using Polly.Registry;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.DataLayerTests
{

    public class DriversDAOTestBase : IClassFixture<DatabaseFixture>
    {
        protected readonly DriversDAO _driversDAO;
        protected readonly string _connectionString;
        protected CancellationTokenSource? _cts;
        protected readonly bool _shouldSkipTests;
        protected readonly ILogger<DriversDAO> _testLogger;

        protected ResiliencePipelineProvider<string> _mockPipelineProvider;
        protected IConfiguration _mockConfiguration;
        protected InMemorySink? memorySink;
        protected ILogger<DriversDAO>? _memoryLogger;
        protected RetryEventService _mockRetryEventService;

        public DriversDAOTestBase(DatabaseFixture fixture, ITestOutputHelper output)
        {
            // Will not use fixture DriversDAO so as to have control over parameters
            _connectionString = fixture.ConnectionString;
            if (fixture.CanConnectToDatabase == false)
            {
                _shouldSkipTests = true;
            }
            _testLogger = SharedFunctions.CreateTestLogger<DriversDAO>(output);

            _mockPipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
            _mockPipelineProvider.GetPipeline("sql-retry-pipeline").Returns(ResiliencePipeline.Empty);
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockRetryEventService = Substitute.For<RetryEventService>();

            _driversDAO = new DriversDAO(_mockPipelineProvider, _mockConfiguration, _testLogger, _connectionString, _mockRetryEventService);
        }

        public void InitializeMemorySinkLogger()
        {
            (ILogger<DriversDAO> MemoryLogger, InMemorySink MemorySink) = SharedFunctions.CreateMemorySinkLogger<DriversDAO>();
            _memoryLogger = MemoryLogger;
            memorySink = MemorySink;
        }
    }

    public class DriversDAOTests(DatabaseFixture fixture, ITestOutputHelper output) : DriversDAOTestBase(fixture, output)
    {
        [SkippableFact]
        public async Task GetAllDriversAsync_ReturnsExpectedDataTable()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();

            // Act
            DataTable? result = await _driversDAO.GetAllDriversAsync(_cts.Token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(105, result.Rows.Count);

            DataRow firstRow = result.Rows[0];
            Assert.Equal(1, firstRow["DriverID"]);
            _testLogger.LogInformation("First Driver: {Name} {Surname}, ID: {DriverID}, EmployeeNo: {EmployeeNo}, LicenseType: {LicenseType}, Availability: {Availability}",
                           firstRow["Name"], firstRow["Surname"], firstRow["DriverID"], firstRow["EmployeeNo"], firstRow["LicenseType"], firstRow["Availability"]);

            DataRow lastRow = result.Rows[104];
            Assert.Equal(105, lastRow["DriverID"]);
            _testLogger.LogInformation("Last Driver: {Name} {Surname}, ID: {DriverID}, EmployeeNo: {EmployeeNo}, LicenseType: {LicenseType}, Availability: {Availability}",
                              lastRow["Name"], lastRow["Surname"], lastRow["DriverID"], lastRow["EmployeeNo"], lastRow["LicenseType"], lastRow["Availability"]);
        }

        [SkippableTheory]
        [InlineData("EMP1234", false)]
        [InlineData("EMP99999", true)]
        public async Task GetEmployeeNoCountAsync_EnsuresValueIsUnique(string EmployeeNo, bool isUnique)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();

            // Act
            int result = await _driversDAO.GetEmployeeNoCountAsync(EmployeeNo);

            // Assert
            if (isUnique)
            {
                Assert.Equal(0, result);
            }
            else
            {
                Assert.Equal(1, result);
            }
        }

        [Fact]
        public async Task GetRecordCountAsync_GetsCorrectRecordCount()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();

            // Act
            int RecordCount = await _driversDAO.GetRecordCountAsync(_cts.Token);

            // Assert
            Assert.Equal(105, RecordCount);
        }

        [SkippableTheory]
        [InlineData(1, "Sarah", "Johnson", "EMP1230", 1, true)]
        [InlineData(2, "Emily", "Jones", "EMP7380", 1, false)]
        [InlineData(3, "Jane", "Davis", "EMP1432", 2, true)]
        [InlineData(4, "David", "Smith", "EMP7070", 2, false)]
        [InlineData(5, "Laura", "Moore", "EMP5849", 3, true)]
        [InlineData(6, "John", "Taylor", "EMP2187", 3, false)]
        public async Task GetDriverByIDAsync_ReturnsCorrectDriver_WhenDriverExists(int DriverID, string Name, string Surname, string EmployeeNo, int LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();

            // Act
            DataTable result = await _driversDAO.GetDriverByIDAsync(DriverID);

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

            _testLogger.LogInformation("Driver: {Name} {Surname}, ID: {DriverID}, EmployeeNo: {EmployeeNo}, LicenseType: {LicenseType}, Availability: {Availability}",
                         firstRow["Name"], firstRow["Surname"], firstRow["DriverID"], firstRow["EmployeeNo"], firstRow["LicenseType"], firstRow["Availability"]);
        }

        [SkippableFact]
        public async Task GetDriverByIDAsync_ReturnsEmptyTable_WhenDriverDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();
            int nonExistentID = 9999;

            // Act
            DataTable result = await _driversDAO.GetDriverByIDAsync(nonExistentID);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Rows);
        }
    }

    [Collection("Sequential")]
    public class SequentialDriversDAOTests(DatabaseFixture fixture, ITestOutputHelper output) : DriversDAOTestBase(fixture, output)
    {
        [SkippableTheory]
        [InlineData(-1, false)] // Cannot have negative page

        // 3 point BVA
        [InlineData(0, false)]  // Out of range
        [InlineData(1, true)]   // Lower bound
        [InlineData(2, true)]   // Lower bound + 1

        [InlineData(4, true)]   // Upper bound - 1
        [InlineData(5, true)]   // Upper bound
        // Was originally using page 6 but TestDB added 5 records for records less than recordlimit situations
        [InlineData(7, false)]  // Out of range
        public async Task GetDriversAtPageAsync_ReturnsExpectedDataTable_ForSpecifiedPage(int SetPageTo, bool IsValid)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();

            // Act
            _testLogger.LogInformation("Getting drivers at page {PageNumber}", SetPageTo);
            DataTable? result = await _driversDAO.GetDriversAtPageAsync(SetPageTo, _cts.Token);

            // Assert
            if (IsValid)
            {
                Assert.NotNull(result);
                Assert.Equal(GlobalConstants.s_recordLimit, result.Rows.Count);

                int ExpectedLastID = SetPageTo * (GlobalConstants.s_recordLimit);
                int ExpectedFirstID = ExpectedLastID - (GlobalConstants.s_recordLimit - 1);
                _testLogger.LogInformation("ExpectedLastID: {ExpectedLastID}", ExpectedLastID);
                _testLogger.LogInformation("ExpectedFirstID: {ExpectedFirstID}", ExpectedFirstID);

                DataRow firstRow = result.Rows[0];
                Assert.Equal(ExpectedFirstID, firstRow["DriverID"]);

                _testLogger.LogInformation("First Driver: {Name} {Surname}, ID: {DriverID}, EmployeeNo: {EmployeeNo}, LicenseType: {LicenseType}, Availability: {Availability}",
                              firstRow["Name"], firstRow["Surname"], firstRow["DriverID"], firstRow["EmployeeNo"], firstRow["LicenseType"], firstRow["Availability"]);

                DataRow lastRow = result.Rows[19];
                Assert.Equal(ExpectedLastID, lastRow["DriverID"]);

                _testLogger.LogInformation("Last Driver: {Name} {Surname}, ID: {DriverID}, EmployeeNo: {EmployeeNo}, LicenseType: {LicenseType}, Availability: {Availability}",
                               lastRow["Name"], lastRow["Surname"], lastRow["DriverID"], lastRow["EmployeeNo"], lastRow["LicenseType"], lastRow["Availability"]);
            }
            else
            {
                if (SetPageTo > 6 && result != null)
                {
                    Assert.Empty(result.Rows);
                }
                else
                {
                    Assert.Null(result);
                }
            }
        }

        [SkippableFact]
        public async Task GetDriversAtPageAsync_ReturnsCorrectDataTable_WhenPageHasFewerThanRecordLimit()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();
            int SetPageTo = 6;

            // Act
            _testLogger.LogInformation("Getting drivers at page {PageNumber}", SetPageTo);
            DataTable? result = await _driversDAO.GetDriversAtPageAsync(SetPageTo, _cts.Token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Rows.Count); // There is 105 records. 105 % 20 = 5 remaining records

            DataRow firstRow = result.Rows[0];
            Assert.Equal(101, firstRow["DriverID"]);
            _testLogger.LogInformation("First Driver: {Name} {Surname}, ID: {DriverID}, EmployeeNo: {EmployeeNo}, LicenseType: {LicenseType}, Availability: {Availability}",
                           firstRow["Name"], firstRow["Surname"], firstRow["DriverID"], firstRow["EmployeeNo"], firstRow["LicenseType"], firstRow["Availability"]);

            DataRow lastRow = result.Rows[4];
            Assert.Equal(105, lastRow["DriverID"]);
            _testLogger.LogInformation("Last Driver: {Name} {Surname}, ID: {DriverID}, EmployeeNo: {EmployeeNo}, LicenseType: {LicenseType}, Availability: {Availability}",
                              lastRow["Name"], lastRow["Surname"], lastRow["DriverID"], lastRow["EmployeeNo"], lastRow["LicenseType"], lastRow["Availability"]);
        }

        [SkippableFact]
        public async Task InsertDriverAsync_ReturnsCorrectDriverID_WhenDriverIsInserted()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();
            DriversDTO mockDriver = new(
            DriverID: 999, // DriverID auto-increments server-side on insert. 999 is a placeholder 
            Name: "Test",
            Surname: "Insert",
            EmployeeNo: "EMP106",
            LicenseType: LicenseType.Code14,
            Availability: false
            );

            // Act
            int returnedDriverID = await _driversDAO.InsertDriverAsync(mockDriver);

            // Assert
            Assert.Equal(106, returnedDriverID);

            // Clean up
            await _driversDAO.DeleteDriverAsync(returnedDriverID);
            await _driversDAO.ReseedTable("Drivers", 105);
        }

        // Running this test in parallel causes multiple driver inserts with IDs above 106, leading to improper cleanup
        [SkippableTheory]
        [InlineData("Olivia", "Clark", "EMP106", LicenseType.Code8, true)]
        [InlineData("Liam", "White", "EMP107", LicenseType.Code8, false)]
        [InlineData("Sophia", "Harris", "EMP108", LicenseType.Code10, true)]
        [InlineData("Noah", "Martin", "EMP109", LicenseType.Code10, false)]
        [InlineData("Ava", "Thompson", "EMP1010", LicenseType.Code14, true)]
        [InlineData("Mason", "Baker", "EMP1011", LicenseType.Code14, false)]
        public async Task InsertDriverAsync_EntersDetailsCorrectly_WhenInserted(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();

            DriversDTO mockDriver = new(
            DriverID: 999, // DriverID auto-increments server-side on insert. 999 is a placeholder 
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            // Act
            int returnedDriverID = await _driversDAO.InsertDriverAsync(mockDriver);

            // Assert
            Assert.True(returnedDriverID > 105, "DriverID should be greater than existing records.");

            DataTable result = await _driversDAO.GetDriverByIDAsync(returnedDriverID);
            DataRow firstRow = result.Rows[0];
            Assert.Equal(Name, firstRow["Name"]);
            Assert.Equal(Surname, firstRow["Surname"]);
            Assert.Equal(EmployeeNo, firstRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, firstRow["LicenseType"]);
            Assert.Equal(Availability, firstRow["Availability"]);

            // Clean up
            await _driversDAO.DeleteDriverAsync(returnedDriverID);
            await _driversDAO.ReseedTable("Drivers", 105);
        }

        [SkippableFact]
        public async Task UpdateDriverAsync_LogsWarning_WhenDriverDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();
            DriversDTO mockDriver = new(
            DriverID: 999,
            Name: "Test",
            Surname: "Update",
            EmployeeNo: "EMP106",
            LicenseType: LicenseType.Code14,
            Availability: false
            );

            string message = $"No driver was found with ID: {mockDriver.DriverID}, update not performed";
            InitializeMemorySinkLogger(); // Make sure this is always before mockDAO 
            DriversDAO mockDAO = new(_mockPipelineProvider, _mockConfiguration, _memoryLogger, _connectionString, _mockRetryEventService);

            // Act
            await mockDAO.UpdateDriverAsync(mockDriver);

            // Assert
            if (memorySink != null)
            {
                if (memorySink.LogEvents.Any())
                {
                    List<LogEvent> memoryLog = memorySink.LogEvents.ToList();
                    Assert.Single(memoryLog);
                    Assert.Equal(LogEventLevel.Warning, memoryLog[0].Level);
                    Assert.Contains(message, memoryLog[0].RenderMessage());
                }
                else
                {
                    Assert.Fail("No log events found.");
                }
            }
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
            _cts = new CancellationTokenSource();
            int DriverID = 105;

            DriversDTO mockDriver = new(
            DriverID: DriverID,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            // Act
            await _driversDAO.UpdateDriverAsync(mockDriver);

            // Assert
            DataTable result = await _driversDAO.GetDriverByIDAsync(DriverID);
            DataRow firstRow = result.Rows[0];
            Assert.Equal(Name, firstRow["Name"]);
            Assert.Equal(Surname, firstRow["Surname"]);
            Assert.Equal(EmployeeNo, firstRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, firstRow["LicenseType"]);
            Assert.Equal(Availability, firstRow["Availability"]);
            _testLogger.LogInformation("Driver: {Name} {Surname}, ID: {DriverID}, EmployeeNo: {EmployeeNo}, LicenseType: {LicenseType}, Availability: {Availability}",
                      firstRow["Name"], firstRow["Surname"], firstRow["DriverID"], firstRow["EmployeeNo"], firstRow["LicenseType"], firstRow["Availability"]);

            // Cleanup
            DriversDTO Driver105 = new(
            DriverID: 105, // DriverID auto-increments server-side on insert. 999 is a placeholder 
            Name: "Lucas",
            Surname: "Miller",
            EmployeeNo: "EMP1234",
            LicenseType: LicenseType.Code8,
            Availability: false
            );
            await _driversDAO.UpdateDriverAsync(Driver105);
        }

        [SkippableFact]
        public async Task DeleteDriverAsync_LogsWarning_WhenDriverDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();
            int DriverID = 999;

            string message = $"No driver was found with ID: {DriverID}, delete not performed";
            InitializeMemorySinkLogger(); // Make sure this is always before mockDAO 
            DriversDAO mockDAO = new(_mockPipelineProvider, _mockConfiguration, _memoryLogger, _connectionString, _mockRetryEventService);

            // Act
            await mockDAO.DeleteDriverAsync(DriverID);

            // Assert
            if (memorySink != null)
            {
                if (memorySink.LogEvents.Any())
                {
                    List<LogEvent> memoryLog = memorySink.LogEvents.ToList();
                    Assert.Single(memoryLog);
                    Assert.Equal(LogEventLevel.Warning, memoryLog[0].Level);
                    Assert.Contains(message, memoryLog[0].RenderMessage());
                }
                else
                {
                    Assert.Fail("No log events found.");
                }
            }
        }

        [SkippableFact]
        public async Task DeleteDriverAsync_DeletesCorrectDriver()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();
            int DriverID = 105;

            // Act
            await _driversDAO.DeleteDriverAsync(DriverID);

            // Assert
            DataTable result = await _driversDAO.GetDriverByIDAsync(DriverID);
            Assert.NotNull(result);
            Assert.Empty(result.Rows);

            // Cleanup
            DriversDTO Driver105 = new(
            DriverID: 105, // DriverID auto-increments server-side on insert. 999 is a placeholder 
            Name: "Lucas",
            Surname: "Miller",
            EmployeeNo: "EMP1234",
            LicenseType: LicenseType.Code8,
            Availability: false
            );
            await _driversDAO.ReseedTable("Drivers", 104);
            await _driversDAO.InsertDriverAsync(Driver105);
        }
    }




}
