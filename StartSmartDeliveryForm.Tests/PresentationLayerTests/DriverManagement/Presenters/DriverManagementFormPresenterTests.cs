using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly;
using Polly.Registry;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Presenters;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement.Presenters
{
    public class DriverManagementFormPresenterTests : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<DriverManagementFormPresenter> _testLogger;

        private DriverManagementForm? _driverManagementForm;

        private readonly DriversDAO _driversDAO;
        private PaginationManager<DriversDTO>? _paginationManager;
        private DriverManagementModel? _driverManagementModel;
        private readonly bool _shouldSkipTests;
        private readonly string _connectionString;

        private ResiliencePipelineProvider<string> _mockPipelineProvider;
        private IConfiguration _mockConfiguration;
        private RetryEventService _mockRetryEventService;

        private DriverManagementFormPresenter? _presenter;
        ILogger<DriverManagementModel> _modelTestLogger;


        public DriverManagementFormPresenterTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<DriverManagementFormPresenter>(output);

            _connectionString = fixture.ConnectionString;
            if (fixture.CanConnectToDatabase == false)
            {
                _shouldSkipTests = true;
            }

            _mockPipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
            _mockPipelineProvider.GetPipeline("sql-retry-pipeline").Returns(ResiliencePipeline.Empty);
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockRetryEventService = Substitute.For<RetryEventService>();

            _modelTestLogger = SharedFunctions.CreateTestLogger<DriverManagementModel>(output);

            ILogger<DriversDAO> driversDAOTestLogger = SharedFunctions.CreateTestLogger<DriversDAO>(output);
            _driversDAO = new DriversDAO(_mockPipelineProvider, _mockConfiguration, driversDAOTestLogger, _connectionString, _mockRetryEventService);
        }

        // Will come back to later. 
        //[Fact]
        //public async Task HandlePageChange_NotifiesViewOfChange_WhichChangesAccordingly()
        //{
        //    // Arrange
        //    var mockFileSystem = new MockFileSystem();

        //    string fakeEditPath = "C:\\Users\\fudge\\source\\repos\\MajorNameless\\StartSmartDelivery\\StartSmartDeliveryForm.Tests\\PresentationLayer\\Images\\EditIcon.png";
        //    string fakeDeletePath = "C:\\Users\\fudge\\source\\repos\\MajorNameless\\StartSmartDelivery\\StartSmartDeliveryForm.Tests\\PresentationLayer\\Images\\DeleteIcon.png";
        //    byte[] minimalPng = [
        //    0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
        //    0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk
        //    0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        //    0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
        //    0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, // IDAT chunk
        //    0x54, 0x78, 0xDA, 0x63, 0x60, 0x00, 0x00, 0x00,
        //    0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00,
        //    0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, // IEND chunk
        //    0x42, 0x60, 0x82
        //    ];

        //    mockFileSystem.AddFile(fakeEditPath, new MockFileData(minimalPng));
        //    mockFileSystem.AddFile(fakeDeletePath, new MockFileData(minimalPng));

        //    _driverManagementForm = new(null, null)
        //    {
        //        FileSystem = mockFileSystem
        //    };

        //    _paginationManager = new("Drivers", _driversDAO);

        //    _driverManagementModel = new(_driversDAO, _paginationManager, _modelTestLogger);
        //    await _driverManagementModel.InitializeAsync();
        //    _presenter = new(_driverManagementForm, _driverManagementModel, _driversDAO, _testLogger);

        //    int page = 3;
        //    await _driverManagementModel.PaginationManager.GoToPage(page);

        //    // Act
        //    _presenter.HandlePageChange(null, EventArgs.Empty);

        //    // Assert
        //    Assert.Equal(_driverManagementForm.DataSource, _driverManagementModel.DgvTable);
        //    Assert.Equal($"{page}", _driverManagementForm.StartPageText);
        //    Assert.Equal($"/{_driverManagementModel.PaginationManager.TotalPages}", _driverManagementForm.EndPageText);
        //}
    }
}
