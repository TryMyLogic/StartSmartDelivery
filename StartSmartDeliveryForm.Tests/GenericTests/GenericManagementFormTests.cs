using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using static StartSmartDeliveryForm.Generics.TableDefinition;
using System.Windows.Forms;
using Xunit.Abstractions;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;

namespace StartSmartDeliveryForm.Tests.GenericTests
{
    public class GenericManagementFormTests
    {
        private readonly ILogger<GenericManagementForm> _testLogger;
        private readonly GenericManagementForm _noMsgBoxGenericManagementForm;
        private GenericManagementForm? _testMsgBoxGenericManagementForm;

        public GenericManagementFormTests(ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<GenericManagementForm>(output);
            _noMsgBoxGenericManagementForm = new GenericManagementForm(
                config: TableConfigs.Drivers, // Use Drivers config for consistency
                logger: _testLogger,
                messageBox: new NoMessageBox()
            );
        }

        [Fact]
        public void DataSource_Get_ReturnsDataTable()
        {
            // Arrange
            DataTable expectedDT = new();
            expectedDT.Columns.Add("TestCol", typeof(string));
            expectedDT.Rows.Add("TestValue");
            _noMsgBoxGenericManagementForm.DgvMain.DataSource = expectedDT;

            // Act
            DataTable result = _noMsgBoxGenericManagementForm.DataSource;

            // Assert
            Assert.Equal(expectedDT, result);
        }

        [Fact]
        public void DataSource_Get_ThrowsInvalidOperationException_WhenNotDataTable()
        {
            // Arrange
            _noMsgBoxGenericManagementForm.DgvMain.DataSource = new object();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _noMsgBoxGenericManagementForm.DataSource);
        }

        [Fact]
        public void DataSource_Set_ChangesDataSourceValue()
        {
            // Arrange
            DataTable dataTable = new();

            // Act
            _noMsgBoxGenericManagementForm.DataSource = dataTable;

            // Assert
            Assert.Equal(dataTable, _noMsgBoxGenericManagementForm.DgvMain.DataSource);
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

            GenericManagementForm form = new(
                config: TableConfigs.Drivers,
                logger: _testLogger,
                messageBox: new NoMessageBox(),
                fileSystem: mockFileSystem
            );

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
            _testMsgBoxGenericManagementForm = new GenericManagementForm(
                config: TableConfigs.Drivers,
                logger: _testLogger,
                messageBox: testMsgBox
            );

            // Act
            _testMsgBoxGenericManagementForm.ShowMessageBox(text, caption, button, icon);

            // Assert
            Assert.True(testMsgBox.WasShowCalled, "Message box was not shown");
            Assert.Equal(text, testMsgBox.LastMessage);
            Assert.Equal(caption, testMsgBox.LastCaption);
            Assert.Equal(button, testMsgBox.LastButton);
            Assert.Equal(icon, testMsgBox.LastIcon);
        }

        [Fact]
        public void UpdatePaginationDisplay_UpdatesDisplayCorrectly()
        {
            // Arrange

            // Act
            _noMsgBoxGenericManagementForm.UpdatePaginationDisplay(5, 10);

            // Assert
            Assert.Equal($"{5}", _noMsgBoxGenericManagementForm.StartPageText);
            Assert.Equal($"/{10}", _noMsgBoxGenericManagementForm.EndPageText);
        }

        [Fact]
        public void ConfigureDataGridViewColumns_SetsColumnsFromTableConfig()
        {
            // Arrange

            // Act
            _noMsgBoxGenericManagementForm.ConfigureDataGridViewColumns();

            // Assert
            DataGridViewColumnCollection columns = _noMsgBoxGenericManagementForm.DgvMain.Columns;
            Assert.Equal(TableConfigs.Drivers.Columns.Count, columns.Count);

            foreach (ColumnConfig columnConfig in TableConfigs.Drivers.Columns)
            {
                DataGridViewColumn column = columns[columnConfig.Name];
                Assert.NotNull(column);
                Assert.Equal(columnConfig.Name, column.Name);
                Assert.Equal(columnConfig.Name, column.HeaderText);
                Assert.Equal(columnConfig.Name, column.DataPropertyName);
                Assert.IsType<DataGridViewTextBoxColumn>(column);
            }
        }

        [Fact]
        public void HideExcludedColumns_HidesPrimaryKeyColumn()
        {
            // Arrange
            GenericManagementForm form = new(TableConfigs.Drivers, _testLogger);
            form.ConfigureDataGridViewColumns(); 

            // Act
            form.HideExcludedColumns();

            // Assert
            DataGridViewColumn primaryKeyColumn = form.DgvMain.Columns[TableConfigs.Drivers.PrimaryKey];
            Assert.NotNull(primaryKeyColumn);
            Assert.False(primaryKeyColumn.Visible);
        }
    }
}
