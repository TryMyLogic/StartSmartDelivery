using System.Drawing.Printing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests
{
    public class GenericPrintDataFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<GenericPrintDataForm> _logger;
        private readonly GenericPrintDataForm _printDataForm;
        private InMemorySink? _memorySink;
        private ILogger<GenericPrintDataForm>? _memoryLogger;

        public GenericPrintDataFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _logger = SharedFunctions.CreateTestLogger<GenericPrintDataForm>(output);
            _printDataForm = new GenericPrintDataForm(_logger);
        }

        internal void InitializeMemorySinkLogger()
        {
            (ILogger<GenericPrintDataForm> MemoryLogger, InMemorySink MemorySink) = SharedFunctions.CreateMemorySinkLogger<GenericPrintDataForm>();
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
            GenericPrintDataForm printDataForm = new(_memoryLogger);

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
