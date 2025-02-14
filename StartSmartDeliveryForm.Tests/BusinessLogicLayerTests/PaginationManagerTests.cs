using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.BusinessLogicLayerTests
{
    public class PaginationManagerTests(DatabaseFixture fixture, ITestOutputHelper output) : IClassFixture<DatabaseFixture>
    {
        private readonly DriversDAO _driversDAO = fixture.DriversDAO;
        private readonly string _connectionString = fixture.ConnectionString;

        [Fact]
        public async Task EmitPageChanged_CallsSubscribers_WhenEventIsSubscribedTo()
        {
            // Arrange
            int PageChangeCalled = 0;
            ILogger<PaginationManager> _mockLogger = Substitute.For<ILogger<PaginationManager>>();
            PaginationManager paginationManager = await PaginationManager.CreateAsync("Drivers", _driversDAO, _mockLogger);

            paginationManager.PageChanged += (currentPage) =>
            {
                PageChangeCalled++;
            };

            // Act
            paginationManager.EmitPageChanged();

            // Assert
            Assert.Equal(1, PageChangeCalled);
        }

        [Fact]
        public void GoToFirstPage_SetsCurrentPageToFirst()
        {
            // Arrange
            PaginationManager paginationManager = new("Drivers", _driversDAO);
            paginationManager.GoToLastPage(); // Needs to be a page other than 1 which is default

            // Act
            paginationManager.GoToFirstPage();

            // Assert
            Assert.Equal(1, paginationManager.CurrentPage);
        }

        [Fact]
        public void GoToLastPage_SetsCurrentPageToLast()
        {
            // Arrange
            PaginationManager paginationManager = new("Drivers", _driversDAO);

            // Act
            paginationManager.GoToLastPage();

            // Assert
            Assert.Equal(paginationManager.TotalPages, paginationManager.CurrentPage);
        }

        [Fact]
        public void GoToNextPage_SetsCurrentPageToNext()
        {
            // Arrange
            PaginationManager paginationManager = new("Drivers", _driversDAO);
            output.WriteLine("Total Pages: " + paginationManager.TotalPages);

            // Act
            paginationManager.GoToNextPage();

            // Assert
            Assert.Equal(2, paginationManager.CurrentPage);
        }

        [Fact]
        public void GoToPreviousPage_SetsCurrentPageToPrevious()
        {
            // Arrange
            PaginationManager paginationManager = new("Drivers", _driversDAO);
            paginationManager.GoToLastPage();
            // Act
            paginationManager.GoToPreviousPage();
            // Assert
            Assert.Equal(paginationManager.TotalPages-1, paginationManager.CurrentPage);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GoToPage_SetsPageToSpecified_WhenValid(int page)
        {
            // Arrange
            PaginationManager paginationManager = new("Drivers", _driversDAO);

            // Act
            paginationManager.GoToPage(page);

            // Assert
            Assert.Equal(page, paginationManager.CurrentPage);
        }

        [Fact]
        public void GoToPage_SetsPageToOne_WhenInvalid()
        {
            // Arrange
            int page = -1;
            PaginationManager paginationManager = new("Drivers", _driversDAO);

            // Act
            paginationManager.GoToPage(page);

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
            Assert.Equal(OriginalRecordCount-1,paginationManager.RecordCount);
        }

        [Theory]
        [InlineData(0,1)]
        [InlineData(2, 2)]
        [InlineData(1,5)] 
        [InlineData(100,6)]
        public void EnsureValidPage_ValidatesPagesCorrectly(int currentPage, int totalPages)
        {
            // Arrange
            PaginationManager paginationManager = new("Drivers", _driversDAO);

            // Use reflection to modify private setters
            typeof(PaginationManager)
                .GetProperty("TotalPages", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
                .SetValue(paginationManager, totalPages);
            output.WriteLine($"Total Pages: {paginationManager.TotalPages}");

            typeof(PaginationManager)
                .GetProperty("CurrentPage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
                .SetValue(paginationManager, currentPage);
            output.WriteLine($"Current Page: {paginationManager.CurrentPage}");

            // Act
            paginationManager.EnsureValidPage();

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
