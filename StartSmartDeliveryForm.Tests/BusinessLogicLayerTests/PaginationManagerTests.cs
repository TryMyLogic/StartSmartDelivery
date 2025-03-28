using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.BusinessLogicLayerTests
{
    public class PaginationManagerTests : IClassFixture<DatabaseFixture>
    {
        private readonly DriversDAO _driversDAO;
        private readonly ITestOutputHelper _output;
        private readonly ILogger<PaginationManager> _testLogger;
        private readonly bool _shouldSkipTests;

        public PaginationManagerTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _driversDAO = fixture.DriversDAO;
            _output = output;

            Serilog.Core.Logger serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output)
            .CreateLogger();

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(serilogLogger);
            });

            _testLogger = loggerFactory.CreateLogger<PaginationManager>();

            if (fixture.CanConnectToDatabase == false)
            {
                _shouldSkipTests = true;
            }
        }

        [SkippableTheory]
        [InlineData("Drivers", false)]
        [InlineData("RandomTableName", true)]
        public async Task GetTotalRecordCount_ThrowsWhenTableIsUnsupported(string TableName, bool ShouldThrow)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = new(TableName, _driversDAO);

            // Act && Assert
            if (ShouldThrow)
            {
                InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => paginationManager.InitializeAsync());
                Assert.IsType<InvalidOperationException>(ex.InnerException);
                Assert.Equal("Total record count not supported for this table", ex.InnerException?.Message);
            }
            else
            {
                await paginationManager.InitializeAsync();
            }
        }

        [SkippableFact]
        public async Task EmitPageChanged_CallsSubscribers_WhenEventIsSubscribedTo()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int PageChangeCalled = 0;
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);

            paginationManager.PageChanged += async (currentPage) =>
            {
                PageChangeCalled++;
                await Task.CompletedTask;
            };

            // Act
            await paginationManager.EmitPageChanged();

            // Assert
            Assert.Equal(1, PageChangeCalled);
        }

        [SkippableFact]
        public async Task GoToFirstPage_SetsCurrentPageToFirst()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();

            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _testLogger);

            await paginationManager.GoToLastPage(); // Needs to be a page other than 1 which is default

            // Act
            await paginationManager.GoToFirstPage();

            // Assert
            Assert.Equal(1, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public async Task GoToLastPage_SetsCurrentPageToLast()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);
            _output.WriteLine("Total Pages: " + paginationManager.TotalPages);

            // Act
            await paginationManager.GoToLastPage();

            // Assert
            Assert.Equal(paginationManager.TotalPages, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public async Task GoToNextPage_SetsCurrentPageToNext()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);
            _output.WriteLine("Total Pages: " + paginationManager.TotalPages);

            // Act
            await paginationManager.GoToNextPage();

            // Assert
            Assert.Equal(2, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public async Task GoToPreviousPage_SetsCurrentPageToPrevious()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);
            await paginationManager.GoToLastPage();
            // Act
            await paginationManager.GoToPreviousPage();
            // Assert
            Assert.Equal(paginationManager.TotalPages - 1, paginationManager.CurrentPage);
        }

        [SkippableTheory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GoToPage_SetsPageToSpecified_WhenValid(int page)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);

            // Act
            await paginationManager.GoToPage(page);

            // Assert
            Assert.Equal(page, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public async Task GoToPage_SetsPageToOne_WhenInvalid()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int page = -1;
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);

            // Act
            await paginationManager.GoToPage(page);

            // Assert
            Assert.Equal(1, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public void UpdateRecordCount_UpdatesRecordCountValueCorrectly()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            PaginationManager paginationManager = new("Drivers", _driversDAO);
            int OriginalRecordCount = paginationManager.RecordCount;

            // Act
            paginationManager.UpdateRecordCount(paginationManager.RecordCount - 1);

            // Assert
            Assert.Equal(OriginalRecordCount - 1, paginationManager.RecordCount);
        }

        [SkippableTheory]
        [InlineData(0, 1)]
        [InlineData(2, 2)]
        [InlineData(1, 5)]
        [InlineData(100, 6)]
        public async Task EnsureValidPage_ValidatesPagesCorrectly(int currentPage, int totalPages)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);

            // Use reflection to modify private setters
            typeof(PaginationManager)
                .GetProperty("TotalPages", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
                .SetValue(paginationManager, totalPages);
            _output.WriteLine("Total Pages: " + paginationManager.TotalPages);

            typeof(PaginationManager)
                .GetProperty("CurrentPage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
                .SetValue(paginationManager, currentPage);
            _output.WriteLine("Current Page: " + paginationManager.CurrentPage);

            // Act
            await paginationManager.EnsureValidPage();

            // Assert
            if (currentPage > totalPages)
            {
                Assert.Equal(paginationManager.TotalPages, paginationManager.CurrentPage);
            }
            else if (currentPage == totalPages)
            {
                Assert.Equal(currentPage, paginationManager.CurrentPage);
            }
            else if (currentPage < 1)
            {
                Assert.Equal(1, paginationManager.CurrentPage); // Clamps to min value 1
            }
            else
            {
                Assert.Equal(currentPage, paginationManager.CurrentPage);
            }

        }
    }
}
