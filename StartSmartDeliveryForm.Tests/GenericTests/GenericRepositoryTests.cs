using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly;
using Polly.Registry;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.Generics;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;
using static StartSmartDeliveryForm.Generics.TableDefinition;

namespace StartSmartDeliveryForm.Tests.GenericTests
{
    public class GenericRepositoryTestBase : IClassFixture<DatabaseFixture>
    {
        protected readonly GenericRepository<DriversDTO> _repository;
        protected readonly string _connectionString;
        protected CancellationTokenSource? _cts;
        protected readonly bool _shouldSkipTests;
        protected readonly ILogger<GenericRepository<DriversDTO>> _testLogger;

        protected ResiliencePipelineProvider<string> _mockPipelineProvider;
        protected IConfiguration _mockConfiguration;
        protected InMemorySink? memorySink;
        protected ILogger<GenericRepository<DriversDTO>>? _memoryLogger;
        protected RetryEventService _mockRetryEventService;

        public GenericRepositoryTestBase(DatabaseFixture fixture, ITestOutputHelper output)
        {
            // Will not use fixture DriversDAO so as to have control over parameters
            _connectionString = fixture.ConnectionString;
            if (fixture.CanConnectToDatabase == false)
            {
                _shouldSkipTests = true;
            }
            _testLogger = SharedFunctions.CreateTestLogger<GenericRepository<DriversDTO>>(output);

            _mockPipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
            _mockPipelineProvider.GetPipeline("sql-retry-pipeline").Returns(ResiliencePipeline.Empty);
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockRetryEventService = Substitute.For<RetryEventService>();

            _repository = new GenericRepository<DriversDTO>(
                _mockPipelineProvider,
                _mockConfiguration,
                TableConfigs.Drivers,
                _testLogger,
                _connectionString,
                _mockRetryEventService
            );
        }

        internal void InitializeMemorySinkLogger()
        {
            (ILogger<GenericRepository<DriversDTO>> MemoryLogger, InMemorySink MemorySink) =
                SharedFunctions.CreateMemorySinkLogger<GenericRepository<DriversDTO>>();
            _memoryLogger = MemoryLogger;
            memorySink = MemorySink;
        }
    }

    public class GenericRepositoryTests(DatabaseFixture fixture, ITestOutputHelper output)
    : GenericRepositoryTestBase(fixture, output)
    {
        [SkippableFact]
        public async Task GetAllRecordsAsync_ReturnsExpectedDataTable()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange

            // Act
            DataTable? result = await _repository.GetAllRecordsAsync();

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
        [InlineData("EMP1234", false)]  // Non-unique
        [InlineData("EMP99999", true)] // Unique
        public async Task GetFieldCountAsync_EnsuresValueCount(string EmployeeNo, bool isUnique)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange

            // Act
            int result = await _repository.GetFieldCountAsync("EmployeeNo", EmployeeNo);

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

        [SkippableFact]
        public async Task GetRecordCountAsync_GetsCorrectRecordCount()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange

            // Act
            int recordCount = await _repository.GetRecordCountAsync();

            // Assert
            Assert.Equal(105, recordCount);
        }

        [SkippableTheory]
        [InlineData(1, "Sarah", "Johnson", "EMP1230", 1, true)]
        [InlineData(2, "Emily", "Jones", "EMP7380", 1, false)]
        [InlineData(3, "Jane", "Davis", "EMP1432", 2, true)]
        [InlineData(4, "David", "Smith", "EMP7070", 2, false)]
        [InlineData(5, "Laura", "Moore", "EMP5849", 3, true)]
        [InlineData(6, "John", "Taylor", "EMP2187", 3, false)]
        public async Task GetRecordByPKAsync_ReturnsCorrectDriver_WhenDriverExists(int DriverID, string Name, string Surname, string EmployeeNo, int LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange

            // Act
            DataTable result = await _repository.GetRecordByPKAsync(DriverID);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);

            DataRow firstRow = result.Rows[0];
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
        public async Task GetRecordByPKAsync_ReturnsEmptyTable_WhenSpecifiedFieldDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int nonExistentId = 9999;

            // Act
            DataTable result = await _repository.GetRecordByPKAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Rows);
        }
    }

    [Collection("Sequential")]
    public class SequentialGenericRepositoryTests(DatabaseFixture fixture, ITestOutputHelper output)
    : GenericRepositoryTestBase(fixture, output)
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
        public async Task GetRecordsAtPageAsync_ReturnsExpectedDataTable_ForSpecifiedPage(int SetPageTo, bool IsValid)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange

            // Act
            _testLogger.LogInformation("Getting drivers at page {PageNumber}", SetPageTo);
            DataTable? result = await _repository.GetRecordsAtPageAsync(SetPageTo);

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
        public async Task GetRecordsAtPageAsync_ReturnsCorrectDataTable_WhenPageHasFewerThanRecordLimit()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int setPageTo = 6;

            // Act
            _testLogger.LogInformation("Getting drivers at page {PageNumber}", setPageTo);
            DataTable? result = await _repository.GetRecordsAtPageAsync(setPageTo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Rows.Count); // 105 records, 105 % 20 = 5 remaining

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
        public async Task InsertRecordAsync_ReturnsCorrectDriverID_WhenDriverIsInserted()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            DriversDTO mockDriver = new(
                DriverID: 999, // DriverID auto-increments server-side on insert. 999 is a placeholder 
                Name: "Test",
                Surname: "Insert",
                EmployeeNo: "EMP106",
                LicenseType: LicenseType.Code14,
                Availability: false
            );

            // Act
            int returnedDriverId = await _repository.InsertRecordAsync(mockDriver);

            // Assert
            Assert.Equal(106, returnedDriverId);

            // Clean up
            await _repository.DeleteRecordAsync(returnedDriverId);
            await _repository.ReseedTableAsync(105);
        }

        // Running this test in parallel causes multiple driver inserts with IDs above 106, leading to improper cleanup
        [SkippableTheory]
        [InlineData("Olivia", "Clark", "EMP106", LicenseType.Code8, true)]
        [InlineData("Liam", "White", "EMP107", LicenseType.Code8, false)]
        [InlineData("Sophia", "Harris", "EMP108", LicenseType.Code10, true)]
        [InlineData("Noah", "Martin", "EMP109", LicenseType.Code10, false)]
        [InlineData("Ava", "Thompson", "EMP1010", LicenseType.Code14, true)]
        [InlineData("Mason", "Baker", "EMP1011", LicenseType.Code14, false)]
        public async Task InsertRecordAsync_EntersDetailsCorrectly_WhenInserted(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            DriversDTO mockDriver = new(
            DriverID: 999,
            Name: Name,
            Surname: Surname,
            EmployeeNo: EmployeeNo,
            LicenseType: LicenseType,
            Availability: Availability
            );

            // Act
            int returnedDriverId = await _repository.InsertRecordAsync(mockDriver);

            // Assert
            Assert.True(returnedDriverId > 105, "DriverID should be greater than existing records.");

            DataTable result = await _repository.GetRecordByPKAsync(returnedDriverId);
            DataRow firstRow = result.Rows[0];
            Assert.Equal(Name, firstRow["Name"]);
            Assert.Equal(Surname, firstRow["Surname"]);
            Assert.Equal(EmployeeNo, firstRow["EmployeeNo"]);
            Assert.Equal((int)LicenseType, firstRow["LicenseType"]);
            Assert.Equal(Availability, firstRow["Availability"]);

            // Clean up
            await _repository.DeleteRecordAsync(returnedDriverId);
            await _repository.ReseedTableAsync(105);
        }

        [SkippableFact]
        public async Task UpdateRecordAsync_LogsWarning_WhenDriverDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            DriversDTO mockDriver = new(
                DriverID: 999,
                Name: "Test",
                Surname: "Update",
                EmployeeNo: "EMP106",
                LicenseType: LicenseType.Code14,
                Availability: false
            );

            string message = $"No record was found with ID: {mockDriver.DriverID}";
            InitializeMemorySinkLogger();
            var mockRepository = new GenericRepository<DriversDTO>(
                _mockPipelineProvider,
                _mockConfiguration,
                TableConfigs.Drivers,
                _memoryLogger,
                _connectionString,
                _mockRetryEventService
            );

            // Act
            await mockRepository.UpdateRecordAsync(mockDriver);

            // Assert
            if (memorySink != null)
            {
                if (memorySink.LogEvents.Any())
                {
                    List<LogEvent> memoryLog = memorySink.LogEvents.ToList();
                    Assert.Single(memoryLog);
                    _testLogger.LogInformation("memory log: {MemoryLog}", string.Join(Environment.NewLine, memoryLog.Select(log => log.RenderMessage())));
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
        public async Task UpdateRecordAsync_UpdatesDetailsCorrectly_WhenDriverExists(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
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
            await _repository.UpdateRecordAsync(mockDriver);

            // Assert
            DataTable result = await _repository.GetRecordByPKAsync(DriverID);
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
                DriverID: 105,
                Name: "Lucas",
                Surname: "Miller",
                EmployeeNo: "EMP1234",
                LicenseType: LicenseType.Code8,
                Availability: false
            );
            await _repository.UpdateRecordAsync(Driver105);
        }

        [SkippableFact]
        public async Task DeleteRecordAsync_LogsWarning_WhenDriverDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int DriverID = 999;

            string message = $"No record found with ID: {DriverID}";
            InitializeMemorySinkLogger();
            var mockRepository = new GenericRepository<DriversDTO>(
                _mockPipelineProvider,
                _mockConfiguration,
                TableConfigs.Drivers,
                _memoryLogger,
                _connectionString,
                _mockRetryEventService
            );

            // Act
            await mockRepository.DeleteRecordAsync(DriverID);

            // Assert
            if (memorySink != null)
            {
                if (memorySink.LogEvents.Any())
                {
                    List<LogEvent> memoryLog = memorySink.LogEvents.ToList();
                    Assert.Single(memoryLog);
                    _testLogger.LogInformation("memory log: {MemoryLog}", string.Join(Environment.NewLine, memoryLog.Select(log => log.RenderMessage())));
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
        public async Task DeleteRecordAsync_DeletesCorrectDriver()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int DriverID = 105;

            // Act
            await _repository.DeleteRecordAsync(DriverID);

            // Assert
            DataTable result = await _repository.GetRecordByPKAsync(DriverID);
            Assert.Empty(result.Rows);

            // Cleanup
            DriversDTO Driver105 = new(
                DriverID: 105,
                Name: "Lucas",
                Surname: "Miller",
                EmployeeNo: "EMP1234",
                LicenseType: LicenseType.Code8,
                Availability: false
            );
            await _repository.ReseedTableAsync(104);
            await _repository.InsertRecordAsync(Driver105);
        }
    }
}
