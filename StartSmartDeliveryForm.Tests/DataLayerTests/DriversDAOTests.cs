using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.SharedLayer.Enums;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.DataLayerTests
{
    public class DriversDAOTests : IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly DriversDAO _driversDAO;
        private readonly string _connectionString;

        public DriversDAOTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _driversDAO = fixture.DriversDAO;
            _connectionString = fixture.ConnectionString;
            _output = output;
        }


    }
}
