using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Serilog.Core;
using Serilog;
using StartSmartDeliveryForm.DataLayer.DAOs;
using Xunit.Abstractions;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.Tests.SharedTestItems;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests
{
    public class DataFormTemplateTests
    {
        private readonly ILogger<DataFormTemplate> _testLogger;
        public DataFormTemplateTests(ITestOutputHelper output) {
            _testLogger = SharedFunctions.CreateTestLogger<DataFormTemplate>(output);


        }
        [Fact]
        public void btnSubmit_Click_EmitsSubmitClickedEvent()
        {

        }

    }
}
