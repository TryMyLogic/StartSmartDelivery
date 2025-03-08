using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;


namespace StartSmartDeliveryForm.Tests.PresentationLayerTests
{
    public class DataFormTemplateTests
    {
        private readonly ILogger<DataFormTemplate> _testLogger;
        private readonly DataFormTemplate _noMsgBoxDataForm;
        private DataFormTemplate? _testMsgDataForm;
        public DataFormTemplateTests(ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<DataFormTemplate>(output);
            _noMsgBoxDataForm = new(_testLogger, new NoMessageBox());
        }

        [Fact]
        public void btnSubmit_Click_RaisesSubmitClickedEvent()
        {
            // Arrange
            bool eventRaised = false;
            EventHandler<SubmissionCompletedEventArgs> eventHandler = (sender, args) =>
            {
                eventRaised = true;
            };
            _noMsgBoxDataForm.SubmitClicked += eventHandler;

            // Act
            _noMsgBoxDataForm.btnSubmit_Click(this, EventArgs.Empty);

            // Assert
            Assert.True(eventRaised);
        }

        [Fact]
        public void btnSubmit_Click_WithoutSubscribers_DoesNotThrowException()
        {
            // Arrange

            // Act
            Exception exception = Record.Exception(() => _noMsgBoxDataForm.btnSubmit_Click(null, EventArgs.Empty));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("Test", "Details", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)]
        [InlineData("Warning", "Alert", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)]
        [InlineData("Error", "Critical", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error)]
        [InlineData("Info", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Information)]
        [InlineData("Question", "Query", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)]
        [InlineData("Stop", "Halt", MessageBoxButtons.RetryCancel, MessageBoxIcon.Stop)]
        public void ShowMessageBox_ShouldShowCorrectDetails(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon)
        {
            // Arrange
            TestMessageBox testMsgBox = new();
            _testMsgDataForm = new(_testLogger, testMsgBox);

            // Act
            _testMsgDataForm.ShowMessageBox(text, caption, button, icon);

            // Assert
            Assert.True(testMsgBox.WasShowCalled, "Message box was not shown");
            Assert.Equal(text, testMsgBox.LastMessage);
            Assert.Equal(caption, testMsgBox.LastCaption);
            Assert.Equal(button, testMsgBox.LastButton);
            Assert.Equal(icon, testMsgBox.LastIcon);
        }
    }
}
