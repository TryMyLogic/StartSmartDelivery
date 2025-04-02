using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Windows.Forms;
using StartSmartDeliveryForm.DataLayer.DAOs;
using Xunit.Abstractions;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using System.Drawing.Printing;
using Serilog.Sinks.InMemory;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models;
using Serilog.Events;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DriverManagement
{
    public class PrintDriverDataFormTests : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<PrintDriverDataForm> _logger;
        private readonly PrintDriverDataForm _printDriverDataForm;
        private InMemorySink? _memorySink;
        private ILogger<PrintDriverDataForm>? _memoryLogger;

        public PrintDriverDataFormTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _logger = SharedFunctions.CreateTestLogger<PrintDriverDataForm>(output);
            _printDriverDataForm = new PrintDriverDataForm(_logger);
        }

        internal void InitializeMemorySinkLogger()
        {
            (ILogger<PrintDriverDataForm> MemoryLogger, InMemorySink MemorySink) = SharedFunctions.CreateMemorySinkLogger<PrintDriverDataForm>();
            _memoryLogger = MemoryLogger;
            _memorySink = MemorySink;
        }

        [Fact]
        public void SetPrintDocument_UpdatesPrintPreviewControlDocument()
        {
            // Arrange
            PrintDocument printDocument = Substitute.For<PrintDocument>();

            // Act
            _printDriverDataForm.SetPrintDocument(printDocument);

            // Assert
            Control? printPreviewControl = _printDriverDataForm.Controls["printPreviewControl"];
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
            PrintDriverDataForm printDriverDataForm = new(_memoryLogger);

            // Act
            printDriverDataForm.HideNavigationButtons();

            // Assert
            SharedFunctions.AssertSingleLogEvent(_memorySink, LogEventLevel.Information, message);
        }

        [Fact]
        public void UpdatePreviewPage_SetsStartpageCorrectly()
        {
            // Arrange
            int PageNumber = 5;

            // Act
            _printDriverDataForm.UpdatePreviewPage(PageNumber);

            // Assert
            Control? printPreviewControl = _printDriverDataForm.Controls["printPreviewControl"];
            Assert.False(printPreviewControl == null, "Could not find printPreviewControl. Should be defined in designer");

            PrintPreviewControl? previewControl = printPreviewControl as PrintPreviewControl;
            Assert.IsType<PrintPreviewControl>(previewControl);

            Assert.Equal(PageNumber, previewControl.StartPage);
        }

        [Fact]
        public void CloseForm_ClosesAndDisposesForm()
        {
            // Act
            _printDriverDataForm.CloseForm();

            // Assert
            Assert.True(_printDriverDataForm.IsDisposed);
        }
    }
}
