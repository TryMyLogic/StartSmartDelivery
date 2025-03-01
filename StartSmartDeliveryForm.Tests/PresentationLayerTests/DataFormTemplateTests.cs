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
using NSubstitute;
using System.Reflection;
using System.Windows.Forms;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;


namespace StartSmartDeliveryForm.Tests.PresentationLayerTests
{
    public class DataFormTemplateTests
    {
        private readonly ILogger<DataFormTemplate> _testLogger;
        public DataFormTemplateTests(ITestOutputHelper output) {
            _testLogger = SharedFunctions.CreateTestLogger<DataFormTemplate>(output);

        }

        // Will do after i am done changing forms to MVP pattern
    }
}
