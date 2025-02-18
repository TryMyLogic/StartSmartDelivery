using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using NSubstitute;
using Serilog;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;
using Serilog.Sinks.XUnit;

namespace StartSmartDeliveryForm.Tests.BusinessLogicLayerTests
{
    public class PaginationManagerTests : IClassFixture<DatabaseFixture>
    {
        private readonly DriversDAO _driversDAO;
        private readonly string _connectionString;
        private readonly ITestOutputHelper _output;

        public PaginationManagerTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _driversDAO = fixture.DriversDAO;
            _connectionString = fixture.ConnectionString;
            _output = output;
        }

        [Fact]
        public async Task GetTotalRecordCount_GetsRecordCountCorrectly()
        {
            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);

            // Act
            _output.WriteLine("Record Count: " + paginationManager.RecordCount);

            // Assert
            Assert.Equal(100, paginationManager.RecordCount);
        }

        [Fact]
        public async Task EmitPageChanged_CallsSubscribers_WhenEventIsSubscribedTo()
        {
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

        [Fact]
        public async Task GoToFirstPage_SetsCurrentPageToFirst()
        {
            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();

            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);

            await paginationManager.GoToLastPage(); // Needs to be a page other than 1 which is default

            // Act
            await paginationManager.GoToFirstPage();

            // Assert
            Assert.Equal(1, paginationManager.CurrentPage);
        }

        [Fact]
        public async Task GoToLastPage_SetsCurrentPageToLast()
        {
            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);
            _output.WriteLine("Total Pages: " + paginationManager.TotalPages);

            // Act
            await paginationManager.GoToLastPage();

            // Assert
            Assert.Equal(paginationManager.TotalPages, paginationManager.CurrentPage);
        }

        [Fact]
        public async Task GoToNextPage_SetsCurrentPageToNext()
        {
            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);
            _output.WriteLine("Total Pages: " + paginationManager.TotalPages);

            // Act
            await paginationManager.GoToNextPage();

            // Assert
            Assert.Equal(2, paginationManager.CurrentPage);
        }

        [Fact]
        public async Task GoToPreviousPage_SetsCurrentPageToPrevious()
        {
            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);
            await paginationManager.GoToLastPage();
            // Act
            await paginationManager.GoToPreviousPage();
            // Assert
            Assert.Equal(paginationManager.TotalPages - 1, paginationManager.CurrentPage);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GoToPage_SetsPageToSpecified_WhenValid(int page)
        {
            // Arrange
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);

            // Act
            await paginationManager.GoToPage(page);

            // Assert
            Assert.Equal(page, paginationManager.CurrentPage);
        }

        [Fact]
        public async Task GoToPage_SetsPageToOne_WhenInvalid()
        {
            // Arrange
            int page = -1;
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);

            // Act
            await paginationManager.GoToPage(page);

            // Assert
            Assert.Equal(1, paginationManager.CurrentPage);
        }

        [Fact]
        public void UpdateRecordCount_UpdatesRecordCountValueCorrectly()
        {
            // Arrange
            PaginationManager paginationManager = new("Drivers", _driversDAO);
            int OriginalRecordCount = paginationManager.RecordCount;

            // Act
            paginationManager.UpdateRecordCount(paginationManager.RecordCount - 1);

            // Assert
            Assert.Equal(OriginalRecordCount - 1, paginationManager.RecordCount);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(2, 2)]
        [InlineData(1, 5)]
        [InlineData(100, 6)]
        public async Task EnsureValidPage_ValidatesPagesCorrectly(int currentPage, int totalPages)
        {
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
