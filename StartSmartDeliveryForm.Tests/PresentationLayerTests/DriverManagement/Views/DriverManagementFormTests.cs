using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using NSubstitute;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement.Views
{
    public class DriverManagementFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<DriverManagementFormTests> _testLogger;
        private readonly DriversDAO _driversDAO;
        private readonly string _connectionString;

        private readonly DriverDataForm _dataform;
        private readonly DriverManagementForm _managementForm;

        public DriverManagementFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<DriverManagementFormTests>(output);
            _driversDAO = fixture.DriversDAO;
            _managementForm = fixture.DriverManagementForm;
            _connectionString = fixture.ConnectionString;
            //  _dataform = new DriverDataForm(_driversDAO);

        }

        [Fact]
        public void UpdatePaginationDisplay_UpdatesDisplayCorrectly()
        {
            TextBox mockTxtStartPage = Substitute.For<TextBox>();
            Label mockLblEndPage = Substitute.For<Label>();

            DriverManagementForm form = new()
            {
                txtStartPage = mockTxtStartPage,
                lblEndPage = mockLblEndPage
            };
        }
    }
}
