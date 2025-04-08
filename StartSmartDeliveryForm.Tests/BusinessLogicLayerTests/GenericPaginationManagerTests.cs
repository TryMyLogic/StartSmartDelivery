using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.BusinessLogicLayerTests
{
    public class GenericPaginationManagerTests(DatabaseFixture fixture, ITestOutputHelper output) : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<GenericPaginationManager<DriversDTO>> _testLogger = SharedFunctions.CreateTestLogger<GenericPaginationManager<DriversDTO>>(output);
        private readonly bool _shouldSkipTests = !fixture.CanConnectToDatabase;
        private readonly IRepository<DriversDTO> _driversRepository = fixture.DriversRepository;

        [SkippableFact]
        public async Task EmitPageChangedAsync_CallsSubscribers_WhenEventIsSubscribedTo()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int pageChangeCalled = 0;
            GenericPaginationManager<DriversDTO> paginationManager = await GenericPaginationManager<DriversDTO>.CreateAsync(_driversRepository, _testLogger);
            paginationManager.PageChanged += async (currentPage) =>
            {
                pageChangeCalled++;
                await Task.CompletedTask;
            };

            // Act
            await paginationManager.EmitPageChangedAsync();

            // Assert
            Assert.Equal(1, pageChangeCalled);
        }

        [SkippableFact]
        public async Task GoToFirstPageAsync_SetsCurrentPageToFirst()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            GenericPaginationManager<DriversDTO> paginationManager = await GenericPaginationManager<DriversDTO>.CreateAsync(_driversRepository, _testLogger);
            await paginationManager.GoToLastPageAsync(); // Needs to be a page other than 1 which is default

            // Act
            await paginationManager.GoToFirstPageAsync();

            // Assert
            Assert.Equal(1, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public async Task GoToLastPageAsync_SetsCurrentPageToLast()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            GenericPaginationManager<DriversDTO> paginationManager = await GenericPaginationManager<DriversDTO>.CreateAsync(_driversRepository, _testLogger);

            // Act
            await paginationManager.GoToLastPageAsync();

            // Assert
            Assert.Equal(paginationManager.TotalPages, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public async Task GoToNextPageAsync_SetsCurrentPageToNext()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            GenericPaginationManager<DriversDTO> paginationManager = await GenericPaginationManager<DriversDTO>.CreateAsync(_driversRepository, _testLogger);

            // Act
            await paginationManager.GoToNextPageAsync();

            // Assert
            Assert.Equal(2, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public async Task GoToPreviousPageAsync_SetsCurrentPageToPrevious()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            GenericPaginationManager<DriversDTO> paginationManager = await GenericPaginationManager<DriversDTO>.CreateAsync(_driversRepository, _testLogger);
            await paginationManager.GoToLastPageAsync();

            // Act
            await paginationManager.GoToPreviousPageAsync();

            // Assert
            Assert.Equal(paginationManager.TotalPages - 1, paginationManager.CurrentPage);
        }

        [SkippableTheory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GoToPageAsync_SetsPageToSpecified_WhenValid(int page)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            GenericPaginationManager<DriversDTO> paginationManager = await GenericPaginationManager<DriversDTO>.CreateAsync(_driversRepository, _testLogger);

            // Act
            await paginationManager.GoToPageAsync(page);

            // Assert
            Assert.Equal(page, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public async Task GoToPageAsync_SetsPageToOne_WhenInvalid()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            int page = -1;
            GenericPaginationManager<DriversDTO> paginationManager = await GenericPaginationManager<DriversDTO>.CreateAsync(_driversRepository, _testLogger);

            // Act
            await paginationManager.GoToPageAsync(page);

            // Assert
            Assert.Equal(1, paginationManager.CurrentPage);
        }

        [SkippableFact]
        public void UpdateRecordCountAsync_UpdatesRecordCountValueCorrectly()
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            GenericPaginationManager<DriversDTO> paginationManager = new(_driversRepository, _testLogger);
            int OriginalRecordCount = paginationManager.RecordCount;

            // Act
            paginationManager.UpdateRecordCountAsync(OriginalRecordCount - 1);

            // Assert
            Assert.Equal(OriginalRecordCount - 1, paginationManager.RecordCount);
        }

        [SkippableTheory]
        [InlineData(0, 1)]
        [InlineData(2, 2)]
        [InlineData(1, 5)]
        [InlineData(100, 6)]
        public async Task EnsureValidPageAsync_ValidatesPagesCorrectly(int currentPage, int totalPages)
        {
            Skip.If(_shouldSkipTests, "Test Database is not available. Skipping this test");

            // Arrange
            GenericPaginationManager<DriversDTO> paginationManager = await GenericPaginationManager<DriversDTO>.CreateAsync(_driversRepository, _testLogger);
            paginationManager.UpdateRecordCountAsync(totalPages * GlobalConstants.s_recordLimit);

            // Uses reflection to modify private setter
            typeof(GenericPaginationManager<DriversDTO>)
                .GetProperty("CurrentPage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
                .SetValue(paginationManager, currentPage);
            _testLogger.LogInformation("Current Page: {CurrentPage}, Total Pages: {TotalPages}",
                paginationManager.CurrentPage, paginationManager.TotalPages);

            // Act
            await paginationManager.EnsureValidPageAsync();

            // Assert
            if (currentPage > totalPages)
            {
                Assert.Equal(paginationManager.TotalPages, paginationManager.CurrentPage);
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
