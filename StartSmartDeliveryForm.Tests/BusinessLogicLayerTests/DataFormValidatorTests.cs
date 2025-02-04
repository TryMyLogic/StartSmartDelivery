using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.DataLayer.DAOs;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.BusinessLogicLayerTests
{
    internal class DataFormValidatorTests : IClassFixture<DatabaseFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly string _connectionString;

        public DataFormValidatorTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _connectionString = fixture.ConnectionString;
            _output = output;
        }


    }
}
