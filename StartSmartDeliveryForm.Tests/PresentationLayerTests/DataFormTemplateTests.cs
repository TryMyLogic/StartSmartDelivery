using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;


namespace StartSmartDeliveryForm.Tests.PresentationLayerTests
{
    public class DataFormTemplateTests
    {
        private readonly ILogger<DataFormTemplate> _testLogger;
        public DataFormTemplateTests(ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<DataFormTemplate>(output);

        }

        // Will do after i am done changing forms to MVP pattern
    }
}
