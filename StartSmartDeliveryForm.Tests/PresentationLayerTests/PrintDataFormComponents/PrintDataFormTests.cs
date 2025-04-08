using System.Drawing.Printing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using StartSmartDeliveryForm.PresentationLayer.PrintDataFormComponents;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.PrintDataFormComponents
{
    public class PrintDataFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<PrintDataForm> _logger;
        private readonly PrintDataForm _printDataForm;
        private InMemorySink? _memorySink;
        private ILogger<PrintDataForm>? _memoryLogger;

        public PrintDataFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _logger = SharedFunctions.CreateTestLogger<PrintDataForm>(output);
            _printDataForm = new PrintDataForm(_logger);
        }

        internal void InitializeMemorySinkLogger()
        {
            (ILogger<PrintDataForm> MemoryLogger, InMemorySink MemorySink) = SharedFunctions.CreateMemorySinkLogger<PrintDataForm>();
            _memoryLogger = MemoryLogger;
            _memorySink = MemorySink;
        }

        [Fact]
        public void SetPrintDocument_UpdatesPrintPreviewControlDocument()
        {
            // Arrange
            PrintDocument printDocument = Substitute.For<PrintDocument>();

            // Act
            _printDataForm.SetPrintDocument(printDocument);

            // Assert
            Control? printPreviewControl = _printDataForm.Controls["printPreviewControl"];
            Assert.False(printPreviewControl == null, "Could not find printPreviewControl. Should be defined in designer");

            PrintPreviewControl? previewControl = printPreviewControl as PrintPreviewControl;
            Assert.IsType<PrintPreviewControl>(previewControl);

            Assert.Equal(printDocument, previewControl.Document);
        }

        [Fact]
        public void HideNavigationButtons_LogsInformation()
        {
            // Arrange
            string message = $"HideNavigationButtons was ran";
            InitializeMemorySinkLogger();
            PrintDataForm printDataForm = new(_memoryLogger);

            // Act
            printDataForm.HideNavigationButtons();

            // Assert
            SharedFunctions.AssertSingleLogEvent(_memorySink, LogEventLevel.Information, message);
        }

        [Fact]
        public void UpdatePreviewPage_SetsStartpageCorrectly()
        {
            // Arrange
            int PageNumber = 5;

            // Act
            _printDataForm.UpdatePreviewPage(PageNumber);

            // Assert
            Control? printPreviewControl = _printDataForm.Controls["printPreviewControl"];
            Assert.False(printPreviewControl == null, "Could not find printPreviewControl. Should be defined in designer");

            PrintPreviewControl? previewControl = printPreviewControl as PrintPreviewControl;
            Assert.IsType<PrintPreviewControl>(previewControl);

            Assert.Equal(PageNumber, previewControl.StartPage);
        }

        [Fact]
        public void CloseForm_ClosesAndDisposesForm()
        {
            // Act
            _printDataForm.CloseForm();

            // Assert
            Assert.True(_printDataForm.IsDisposed);
        }
    }
}
