using System.Data;
using System.Drawing;
using System.IO.Abstractions.TestingHelpers;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.PresentationLayer.TemplatePresenters;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.Enums;
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

        //[Theory]
        //// Valid cases for filtering
        //[InlineData("DriverID", "1", 1)]

        //// Invalid cases (no matches)
        //[InlineData("DriverID", "5", 0)]

        //// Null input
        //[InlineData("DriverID", null, 0)]
        //public void ApplyFilter_ShouldFilterDriverID(string selectedOption, string? searchTerm, int expectedRowCount)
        //{
        //    // Arrange
        //    DataTable dataTable = new();
        //    dataTable.Columns.Add("DriverID", typeof(int));
        //    dataTable.Columns.Add("Name", typeof(string));
        //    dataTable.Columns.Add("LicenseType", typeof(int));
        //    dataTable.Columns.Add("Availability", typeof(bool));

        //    dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);
        //    dataTable.Rows.Add(2, "Jane", (int)LicenseType.Code10, false);
        //    dataTable.Rows.Add(3, "John", (int)LicenseType.Code14, true);
        //    dataTable.Rows.Add(4, "Doe", (int)LicenseType.Code8, false);

        //    ManagementFormTemplate form = new();
        //    form.OverrideDatagridView(dataTable);

        //    // Act
        //    form.ApplyFilter(form.GetDatagridViewTable(), selectedOption, searchTerm);
        //    List<DataRow> filteredRows = [.. form.GetDatagridViewTable().AsEnumerable().Where(row => row.RowState != DataRowState.Deleted)];

        //    Assert.Equal(expectedRowCount, filteredRows.Count);
        //    if (expectedRowCount > 0)
        //    {
        //        Assert.Single(filteredRows); //Each DriverID is unique
        //    }
        //}

        //[Theory]
        //// Valid cases for filtering
        //[InlineData("Name", "John", false, 2)]

        //// Invalid cases (no matches)
        //[InlineData("Name", "Tom", false, 0)]

        //// Case sensitive string matching
        //[InlineData("Name", "JoHn", true, 0)]

        //// Null input
        //[InlineData("Name", "null", false, 1)]
        //public void ApplyFilter_ShouldFilterName(string selectedOption, string searchTerm, bool isCaseSensitive, int expectedRowCount)
        //{
        //    // Arrange
        //    DataTable dataTable = new();
        //    dataTable.Columns.Add("DriverID", typeof(int));
        //    dataTable.Columns.Add("Name", typeof(string));
        //    dataTable.Columns.Add("LicenseType", typeof(int));
        //    dataTable.Columns.Add("Availability", typeof(bool));

        //    dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);
        //    dataTable.Rows.Add(2, "Jane", (int)LicenseType.Code10, false);
        //    dataTable.Rows.Add(3, "John", (int)LicenseType.Code14, true);
        //    dataTable.Rows.Add(4, "null", (int)LicenseType.Code8, false);

        //    ManagementFormTemplate form = new()
        //    {
        //        isCaseSensitive = isCaseSensitive
        //    };
        //    form.OverrideDatagridView(dataTable);

        //    // Act
        //    form.ApplyFilter(form.GetDatagridViewTable(), selectedOption, searchTerm);
        //    List<DataRow> filteredRows = [.. form.GetDatagridViewTable().AsEnumerable().Where(row => row.RowState != DataRowState.Deleted)];

        //    // Assert 
        //    Assert.Equal(expectedRowCount, filteredRows.Count);
        //    if (expectedRowCount > 0 && searchTerm != "null")
        //    {
        //        Assert.Contains("John", filteredRows.Select(row => row["Name"].ToString()));
        //    }
        //    else if (searchTerm == "null")
        //    {
        //        Assert.Contains("null", filteredRows.Select(row => row["Name"].ToString()));
        //    }
        //}

        //[Theory]
        //// Valid cases for filtering
        //[InlineData("LicenseType", "Code8", false, 1)]
        //[InlineData("LicenseType", "1", false, 1)]

        //// Invalid cases (no matches)
        //[InlineData("LicenseType", "CoDe8", true, 0)]
        //[InlineData("LicenseType", "4", false, 0)]

        //// Null input
        //[InlineData("LicenseType", null, false, 0)]
        //public void ApplyFilter_ShouldFilterLicenseType(string selectedOption, string? searchTerm, bool isCaseSensitive, int expectedRowCount)
        //{
        //    // Arrange
        //    DataTable dataTable = new();
        //    dataTable.Columns.Add("DriverID", typeof(int));
        //    dataTable.Columns.Add("Name", typeof(string));
        //    dataTable.Columns.Add("LicenseType", typeof(int));
        //    dataTable.Columns.Add("Availability", typeof(bool));

        //    dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);

        //    ManagementFormTemplate form = new()
        //    {
        //        isCaseSensitive = isCaseSensitive
        //    };
        //    form.OverrideDatagridView(dataTable);

        //    // Act
        //    form.ApplyFilter(form.GetDatagridViewTable(), selectedOption, searchTerm);
        //    List<DataRow> filteredRows = [.. form.GetDatagridViewTable().AsEnumerable().Where(row => row.RowState != DataRowState.Deleted)];

        //    // Assert 
        //    Assert.Equal(expectedRowCount, filteredRows.Count);
        //    if (expectedRowCount > 0)
        //    {
        //        Assert.Contains("1", filteredRows.Select(row => row["LicenseType"].ToString()));
        //    }
        //}

        //[Theory]
        //// Valid cases for filtering
        //[InlineData("Availability", "True", false, 1)]
        //[InlineData("Availability", "1", false, 1)]
        //[InlineData("Availability", "False", false, 1)]
        //[InlineData("Availability", "0", false, 1)]

        ////Case sensitivity (Shouldnt apply to bools)
        //[InlineData("Availability", "TrUe", true, 1)]

        //// Invalid cases (no matches)
        //[InlineData("Availability", "Invalid", false, 0)]
        //[InlineData("Availability", "2", false, 0)]

        //// Null input
        //[InlineData("Availability", null, false, 0)]
        //public void ApplyFilter_ShouldFilterAvailability(string selectedOption, string? searchTerm, bool isCaseSensitive, int expectedRowCount)
        //{
        //    // Arrange
        //    DataTable dataTable = new();
        //    dataTable.Columns.Add("DriverID", typeof(int));
        //    dataTable.Columns.Add("Name", typeof(string));
        //    dataTable.Columns.Add("LicenseType", typeof(int));
        //    dataTable.Columns.Add("Availability", typeof(bool));

        //    dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);
        //    dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, false);

        //    ManagementFormTemplate form = new()
        //    {
        //        isCaseSensitive = isCaseSensitive
        //    };
        //    form.OverrideDatagridView(dataTable);

        //    // Act
        //    form.ApplyFilter(form.GetDatagridViewTable(), selectedOption, searchTerm);
        //    List<DataRow> filteredRows = [.. form.GetDatagridViewTable().AsEnumerable().Where(row => row.RowState != DataRowState.Deleted)];

        //    // Assert 
        //    Assert.Equal(expectedRowCount, filteredRows.Count);
        //    if (expectedRowCount > 0 && searchTerm != null)
        //    {
        //        bool expectedBoolValue =
        //   searchTerm.Equals("True", StringComparison.OrdinalIgnoreCase) || searchTerm.Equals("1", StringComparison.OrdinalIgnoreCase);

        //        // Assert that all the filtered rows match the expected Availability value
        //        Assert.True(filteredRows.All(row => row.Field<bool>("Availability") == expectedBoolValue));
        //    }
        //}

        //[Theory]
        //// Valid cases for filtering (case in-sensitive)
        //[InlineData("DriverID", "1", false, 1)]
        //[InlineData("Name", "John", false, "John")]
        //[InlineData("LicenseType", "Code8", false, (int)LicenseType.Code8)]
        //[InlineData("Availability", "True", false, true)]

        //// Invalid cases (no matches)
        //[InlineData("DriverID", "", false, null)]
        //[InlineData("Name", "", false, null)]
        //[InlineData("LicenseType", "", false, null)]
        //[InlineData("Availability", "", false, null)]

        //// Boolean values as numbers and different cases
        ////[InlineData("Availability", "True", false, true)] - Already tested
        //[InlineData("Availability", "False", false, false)]
        //[InlineData("Availability", "1", false, true)]
        //[InlineData("Availability", "0", false, false)]

        //// Case sensitive string matching
        //[InlineData("Name", "JoHn", true, null)]

        //// Case insensitive filtering
        //[InlineData("Name", "john", false, "John")]

        //// Invalid column test
        //[InlineData("InvalidColumn", "Test", false, null)]

        //// Special characters 
        //[InlineData("Name", "O'Reilly", false, "O'Reilly")]

        //// Null input test
        //[InlineData("Name", null, false, null)] //searchTerm would never be null but testing anyways
        //[InlineData("Name", "null", false, "null")] //For tables that allow null inputs

        //public void FilterRows_ShouldReturnCorrectRows(string selectedOption, string? searchTerm, bool isCaseSensitive, object? expectedValue)
        //{
        //    // Arrange
        //    DataTable dataTable = new();
        //    dataTable.Columns.Add("DriverID", typeof(int));
        //    dataTable.Columns.Add("Name", typeof(string));
        //    dataTable.Columns.Add("LicenseType", typeof(int));
        //    dataTable.Columns.Add("Availability", typeof(bool));

        //    // Adding sample rows to DataTable
        //    dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);
        //    dataTable.Rows.Add(2, "O'Reilly", (int)LicenseType.Code10, false);
        //    dataTable.Rows.Add(2, "null", (int)LicenseType.Code14, false);


        //    // Act
        //    List<DataRow> filteredRows = ManagementFormTemplate.FilterRows(dataTable, selectedOption, searchTerm, isCaseSensitive);

        //    // Assert
        //    if (expectedValue == null)
        //    {
        //        if (searchTerm == "null")
        //        {
        //            Assert.Equal(expectedValue, filteredRows[0][selectedOption]);
        //        }
        //        // Expect no matches, so filteredRows should be empty
        //        Assert.Empty(filteredRows);
        //    }
        //    else if (searchTerm != null && selectedOption == "Availability" &&
        //            (searchTerm.Equals("False", StringComparison.OrdinalIgnoreCase) || searchTerm.Equals("0")))
        //    {
        //        Assert.Equal(2, filteredRows.Count);
        //    }
        //    else
        //    {
        //        Assert.Single(filteredRows);
        //        Assert.Equal(expectedValue, filteredRows[0][selectedOption]);
        //    }

        //}

        //[Fact]
        //public void SettingIsCaseSensitive_CanOverrideDefaultValue()
        //{
        //    // Arrange
        //    bool expectedSensitivity = true;

        //    // Act
        //    ManagementFormTemplate form = new()
        //    {
        //        isCaseSensitive = expectedSensitivity
        //    };

        //    // Assert
        //    Assert.Equal(expectedSensitivity, form.isCaseSensitive);
        //}
    }
}
