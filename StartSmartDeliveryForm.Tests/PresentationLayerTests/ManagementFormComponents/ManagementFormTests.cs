﻿using System.Data;
using System.Drawing;
using System.IO.Abstractions.TestingHelpers;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.ManagementFormComponents
{
    public class ManagementFormTests
    {
        private readonly ILogger<ManagementForm> _testLogger;
        private readonly ManagementForm _noMsgBoxManagementForm;
        private ManagementForm? _testMsgBoxManagementForm;

        public ManagementFormTests(ITestOutputHelper output)
        {
            _testLogger = SharedFunctions.CreateTestLogger<ManagementForm>(output);
            _noMsgBoxManagementForm = new ManagementForm(
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
            _noMsgBoxManagementForm.DgvMain.DataSource = expectedDT;

            // Act
            DataTable result = _noMsgBoxManagementForm.DataSource;

            // Assert
            Assert.Equal(expectedDT, result);
        }

        [Fact]
        public void DataSource_Get_ThrowsInvalidOperationException_WhenNotDataTable()
        {
            // Arrange
            _noMsgBoxManagementForm.DgvMain.DataSource = new object();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _noMsgBoxManagementForm.DataSource);
        }

        [Fact]
        public void DataSource_Set_ChangesDataSourceValue()
        {
            // Arrange
            DataTable dataTable = new();

            // Act
            _noMsgBoxManagementForm.DataSource = dataTable;

            // Assert
            Assert.Equal(dataTable, _noMsgBoxManagementForm.DgvMain.DataSource);
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

            byte[] pngBytes = [
            0x89, // Non-ASCII byte prevents misinterpretation as a text file
            0x50, 0x4E, 0x47, // "P,N,G" in ASCII
            0x0D,
            0x0A,
            0x1A,
            0x0A
            ];

            mockFileSystem.AddFile(editPath, new MockFileData(pngBytes));
            mockFileSystem.AddFile(deletePath, new MockFileData(pngBytes));

            ManagementForm form = new(
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
            _testMsgBoxManagementForm = new ManagementForm(
                config: TableConfigs.Drivers,
                logger: _testLogger,
                messageBox: testMsgBox
            );

            // Act
            _testMsgBoxManagementForm.ShowMessageBox(text, caption, button, icon);

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
            _noMsgBoxManagementForm.UpdatePaginationDisplay(5, 10);

            // Assert
            Assert.Equal($"{5}", _noMsgBoxManagementForm.StartPageText);
            Assert.Equal($"/{10}", _noMsgBoxManagementForm.EndPageText);
        }

        [Fact]
        public void ConfigureDataGridViewColumns_SetsColumnsFromTableConfig()
        {
            // Arrange

            // Act
            _noMsgBoxManagementForm.ConfigureDataGridViewColumns();

            // Assert
            DataGridViewColumnCollection columns = _noMsgBoxManagementForm.DgvMain.Columns;
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
            ManagementForm form = new(TableConfigs.Drivers, _testLogger);
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
