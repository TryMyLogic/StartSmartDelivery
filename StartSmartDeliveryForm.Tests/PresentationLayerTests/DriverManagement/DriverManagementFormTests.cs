using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using Xunit.Abstractions;
using static Azure.Core.HttpHeader;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement
{
    public class DriverManagementFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly DriversDAO _driversDAO;
        private readonly string _connectionString;

        private readonly DriverDataForm _dataform;
        private readonly DriverManagementForm _managementForm;

        public DriverManagementFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _driversDAO = fixture.DriversDAO;
            _managementForm = fixture.DriverManagementForm;
            _connectionString = fixture.ConnectionString;
            _dataform = new DriverDataForm(_driversDAO);
            _output = output;
        }


    }
}
