using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.PresentationLayer.TemplatePresenters;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.Template_Presenters
{
    public class ManagementFormPresenterTemplateTests
    {
        private readonly ILogger<ManagementFormPresenterTemplate> _testLogger;
        public ManagementFormPresenterTemplateTests(ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<ManagementFormPresenterTemplate>(output);
            ManagementFormTemplate _noMsgBoxManagementFormTemplate = new(null, new NoMessageBox());

        }
    }
}
