using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.PresentationLayer.TemplatePresenters;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.Template_Presenters
{
    public class DataFormPresenterTemplateTests
    {
        private readonly ILogger<DataFormPresenterTemplate> _testLogger;
        private readonly DataFormTemplate _dataFormTemplate;
        private DataFormPresenterTemplate? _presenterTemplate;

        public DataFormPresenterTemplateTests(ITestOutputHelper output)
        {
            ILogger<DataFormTemplate> _dataFormTestLogger = SharedFunctions.CreateTestLogger<DataFormTemplate>(output);
            _dataFormTemplate = new(_dataFormTestLogger, new NoMessageBox());

            _testLogger = SharedFunctions.CreateTestLogger<DataFormPresenterTemplate>(output);
        }

        // TODO: Will come back to when i have a way to indirectly await .HandleSubmit_Clicked. Else test always fails due to Async void
        //[Fact]
        //public void HandleSubmit_Clicked_RaisesSubmissionCompletedEvent()
        //{
        //    // Arrange
        //    _presenterTemplate = new DataFormPresenterTemplate(_dataFormTemplate, _testLogger);

        //    bool eventRaised = false;
        //    void eventHandler(object? sender, SubmissionCompletedEventArgs args)
        //    {
        //        eventRaised = true;
        //    }
        //    _presenterTemplate.SubmissionCompleted += eventHandler;

        //    // Act
        //    _presenterTemplate.HandleSubmit_Clicked(null, EventArgs.Empty);

        //    // Assert
        //    Assert.True(eventRaised);

        //    // Cleanup
        //    _presenterTemplate.SubmissionCompleted -= eventHandler;
        //}

    }
}
