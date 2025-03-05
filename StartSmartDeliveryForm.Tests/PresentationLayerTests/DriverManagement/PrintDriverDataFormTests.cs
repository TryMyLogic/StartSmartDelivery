﻿using StartSmartDeliveryForm.DataLayer.DAOs;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement
{
    internal class PrintDriverDataFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly DriversDAO _driversDAO;
        private readonly string _connectionString;

        public PrintDriverDataFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _driversDAO = fixture.DriversDAO;
            _connectionString = fixture.ConnectionString;
            _output = output;
        }


    }
}
