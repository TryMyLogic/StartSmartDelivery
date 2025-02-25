using System.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly.Registry;
using Polly;
using Serilog;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer;
using Xunit.Abstractions;
using Microsoft.Extensions.Configuration;

namespace StartSmartDeliveryForm.Tests.DataLayerTests
{

    [Collection("Sequential")]
    public class DriversDAOTests : IClassFixture<DatabaseFixture>
    {
        private readonly DriversDAO _driversDAO;
        private readonly string _connectionString;
        private CancellationTokenSource? _cts;
        private readonly bool _shouldSkipTests;
        private readonly ILogger<DriversDAO> _testLogger;

        public DriversDAOTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            // Will not use fixture DriversDAO so as to have control over parameters
            _connectionString = fixture.ConnectionString;
            if (fixture.CanConnectToDatabase == false)
            {
                _shouldSkipTests = true;
            }

            Serilog.Core.Logger serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output)
            .CreateLogger();

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(serilogLogger);
            });

            _testLogger = loggerFactory.CreateLogger<DriversDAO>();

            ResiliencePipelineProvider<string> _mockPipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
            _mockPipelineProvider.GetPipeline("sql-retry-pipeline").Returns(ResiliencePipeline.Empty);
            IConfiguration _mockConfiguration = Substitute.For<IConfiguration>();
            ILogger<DriversDAO> _mockLogger = Substitute.For<ILogger<DriversDAO>>();
            RetryEventService _mockRetryEventService = Substitute.For<RetryEventService>();

            _driversDAO = new DriversDAO(_mockPipelineProvider, _mockConfiguration, _testLogger, _connectionString, _mockRetryEventService);
        }

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

        [SkippableTheory]
        [InlineData(1, "Sarah", "Johnson", "EMP1230", 1, true)]
        [InlineData(2, "Emily", "Jones", "EMP7380", 1, false)]
        [InlineData(3, "Jane", "Davis", "EMP1432", 2, true)]
        [InlineData(4, "David", "Smith", "EMP7070", 2, false)]
        [InlineData(5, "Laura", "Moore", "EMP5849", 3, true)]
        [InlineData(6, "John", "Taylor", "EMP2187", 3, false)]
        public async Task GetDriverByID_ReturnsCorrectDriver_WhenDriverExists(int DriverID, string Name, string Surname, string EmployeeNo, int LicenseType, bool Availability)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();

            // Act
            DataTable result = await _driversDAO.GetDriverByIDAsync(DriverID, _cts.Token);

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
        public async Task GetDriverByID_ReturnsEmptyTable_WhenDriverDoesNotExist()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            _cts = new CancellationTokenSource();
            int nonExistentID = 9999;

            // Act
            DataTable result = await _driversDAO.GetDriverByIDAsync(nonExistentID, _cts.Token);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Rows);
        }

        


    }
}
