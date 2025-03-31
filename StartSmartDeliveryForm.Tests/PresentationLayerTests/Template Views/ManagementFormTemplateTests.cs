using System.Data;
using System.Drawing;
using System.IO.Abstractions.TestingHelpers;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests
{
    public class ManagementFormTemplateTests
    {
        private readonly ILogger<ManagementFormTemplate> _testLogger;
        private readonly ManagementFormTemplate _noMsgBoxManagementFormTemplate;
        private ManagementFormTemplate? _testMsgBoxManagementFormTemplate;

        public ManagementFormTemplateTests(ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<ManagementFormTemplate>(output);
            _noMsgBoxManagementFormTemplate = new(_testLogger, new NoMessageBox());
        }

        [Fact]
        public void DataSource_Get_ReturnsDataTable()
        {
            // Arrange
            DataTable expectedDT = new();
            expectedDT.Columns.Add("TestCol", typeof(string));
            expectedDT.Rows.Add("TestValue");
            _noMsgBoxManagementFormTemplate.DgvMain.DataSource = expectedDT;

            // Act
            DataTable result = _noMsgBoxManagementFormTemplate.DataSource;

            // Assert
            Assert.Equal(expectedDT, result);
        }

        [Fact]
        public void DataSource_Get_ThrowsInvalidOperationException_WhenNotDatatable()
        {
            // Arrange
            ManagementFormTemplate form = new();
            _noMsgBoxManagementFormTemplate.DgvMain.DataSource = new object();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => form.DataSource);
        }

        [Fact]
        public void DataSource_Set_ChangesDataSourceValue()
        {
            // Arrange
            DataTable dataTable = new();

            // Act
            _noMsgBoxManagementFormTemplate.DataSource = dataTable;

            // Assert
            Assert.Equal(dataTable, _noMsgBoxManagementFormTemplate.DgvMain.DataSource);
        }

        [Fact]
        public void AddEditDeleteButtons_AddsEditAndDeleteColumns()
        {
            // Arrange
            MockFileSystem mockFileSystem = new();
            string editPath = mockFileSystem.Path.GetFullPath(
                mockFileSystem.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "PresentationLayer", "Images", "EditIcon.png"));
            string deletePath = mockFileSystem.Path.GetFullPath(
                mockFileSystem.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "PresentationLayer", "Images", "DeleteIcon.png"));

            byte[] pngBytes = {
            0x89, // Non-ASCII byte prevents misinterpretation as a text file
            0x50, 0x4E, 0x47, // "P,N,G" in ASCII
            0x0D,
            0x0A,
            0x1A,
            0x0A
            };

            mockFileSystem.AddFile(editPath, new MockFileData(pngBytes));
            mockFileSystem.AddFile(deletePath, new MockFileData(pngBytes));

            NoMessageBox noMessage = new();
            ManagementFormTemplate form = new(_testLogger, noMessage, mockFileSystem);

            static Image MockImageLoader(string _) => new Bitmap(1, 1);

            // Act
            form.AddEditDeleteButtons(MockImageLoader);

            // Assert
            DataGridViewColumnCollection columns = form.DgvMain.Columns;
            Assert.Equal(2, columns.Count);

            var editColumn = columns["Edit"] as DataGridViewImageColumn;
            Assert.NotNull(editColumn);
            Assert.Equal("Edit", editColumn.Name);
            Assert.Equal("", editColumn.HeaderText);
            Assert.NotNull(editColumn.Image);
            Assert.Equal(30, editColumn.Width);
            Assert.Equal(DataGridViewAutoSizeColumnMode.None, editColumn.AutoSizeMode);

            var deleteColumn = columns["Delete"] as DataGridViewImageColumn;
            Assert.NotNull(deleteColumn);
            Assert.Equal("Delete", deleteColumn.Name);
            Assert.Equal("", deleteColumn.HeaderText);
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
            _testMsgBoxManagementFormTemplate = new(_testLogger, testMsgBox);

            // Act
            _testMsgBoxManagementFormTemplate.ShowMessageBox(text, caption, button, icon);

            // Assert
            Assert.True(testMsgBox.WasShowCalled, "Message box was not shown");
            Assert.Equal(text, testMsgBox.LastMessage);
            Assert.Equal(caption, testMsgBox.LastCaption);
            Assert.Equal(button, testMsgBox.LastButton);
            Assert.Equal(icon, testMsgBox.LastIcon);
        }
    }
}
