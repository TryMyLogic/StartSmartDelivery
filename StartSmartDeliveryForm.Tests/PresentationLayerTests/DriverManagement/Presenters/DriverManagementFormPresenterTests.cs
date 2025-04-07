using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly;
using Polly.Registry;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Presenters;
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

        private readonly ResiliencePipelineProvider<string> _mockPipelineProvider;
        private readonly IConfiguration _mockConfiguration;
        private readonly RetryEventService _mockRetryEventService;

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

    }
}
