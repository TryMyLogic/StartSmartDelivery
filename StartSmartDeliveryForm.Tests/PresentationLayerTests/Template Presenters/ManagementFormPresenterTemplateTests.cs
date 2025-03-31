using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.PresentationLayer.TemplateModels;
using StartSmartDeliveryForm.PresentationLayer.TemplatePresenters;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.Template_Presenters
{
    public class ManagementFormPresenterTemplateTests
    {
        private readonly ILogger<ManagementFormPresenterTemplate> _testLogger;
        private readonly ManagementFormPresenterTemplate _presenterTemplate;
        public ManagementFormPresenterTemplateTests(ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<ManagementFormPresenterTemplate>(output);
            ManagementFormTemplate _noMsgBoxManagementFormTemplate = new(null, new NoMessageBox());
            ManagementModel model = new();
            _presenterTemplate = new(_noMsgBoxManagementFormTemplate, model, _testLogger);
        }

        // Currently nothing public to test.
    }
}
