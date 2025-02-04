using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.DataLayer.DAOs;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement
{
    internal class DriverDataFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly DriversDAO _driversDAO;
        private readonly string _connectionString;

        public DriverDataFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _driversDAO = fixture.DriversDAO;
            _connectionString = fixture.ConnectionString;
            _output = output;
        }


    }
}
