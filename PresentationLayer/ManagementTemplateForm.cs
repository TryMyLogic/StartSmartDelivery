using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.IdentityModel.Tokens;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace StartSmartDeliveryForm.PresentationLayer
{
    public partial class ManagementTemplateForm : Form
    {
        public ManagementTemplateForm()
        {
            InitializeComponent();
        }

        private static Bitmap ResizeImage(Image img, int width, int height)
        {
            Bitmap resizedImage = new(img, new Size(width, height));
            return resizedImage;
        }

        public static readonly Color MintGreen = Color.FromArgb(73, 173, 72);
        public static readonly Color SoftBeige = Color.FromArgb(240, 221, 188);
        public void SetTheme()
        {
            tsSearchbar.BackColor = SoftBeige;
            pnlGap.BackColor = SoftBeige;

            btnFirst.BackColor = MintGreen;
            btnPrevious.BackColor = MintGreen;
            btnNext.BackColor = MintGreen;
            btnLast.BackColor = MintGreen;
            btnPrint.BackColor = SoftBeige;
            btnAdd.BackColor = SoftBeige;

            btnFirst.FlatAppearance.BorderSize = 0;
            btnPrevious.FlatAppearance.BorderSize = 0;
            btnNext.FlatAppearance.BorderSize = 0;
            btnLast.FlatAppearance.BorderSize = 0;
            btnPrint.FlatAppearance.BorderSize = 0;
            btnAdd.FlatAppearance.BorderSize = 0;

            //Prevents image from touching borders
            btnFirst.Image = ResizeImage(btnFirst.Image, 20, 20);
            btnLast.Image = ResizeImage(btnLast.Image, 20, 20);
        }

        //DO NOT use this in the ManagementTemplateForm_Load.It interferes with children initialization, breaking the child designer. 
        protected void AdjustDataGridViewHeight(DataGridView dataGridView)
        {
            // Set the maximum record limit to 30 or the global constant, whichever is smaller
            int records = Math.Min(GlobalConstants.s_recordLimit, 30); // Select the minimum of the two values

            // Get the row height
            int rowHeight = dataGridView.RowTemplate.Height;

            // Calculate the required height for the DataGridView (rows + column headers)
            int requiredHeight = rowHeight * (records + 1) + dataGridView.ColumnHeadersHeight + 2;

            // Calculate the new height for the parent container (form or panel)
            int formHeightWithoutDataGridView = dataGridView.Parent.Height - dataGridView.Height;

            // Adjust the new height to fit the calculated required height
            int newHeight = formHeightWithoutDataGridView + requiredHeight;

            // Set the new height of the parent container (Form or Panel)
            dataGridView.Parent.Height = newHeight;

            // Optionally log the values for debugging
            FormConsole.Instance.Log($"Row Height: {rowHeight}, Column Headers Height: {dataGridView.ColumnHeadersHeight}, New Height: {newHeight}");
        }

        private void ManagementTemplateForm_Load(object sender, EventArgs e)
        {
            SetTheme();
            dgvMain.RowHeadersVisible = false; // Hides Row Number Column
        }

        protected virtual HashSet<string> GetExcludedColumns()
        {
            return []; // By default, exclude nothing.
        }

        protected void SetSearchOptions(Type Dto)
        {
            cboSearchOptions.Items.Clear();

            //Makes comparison case insensitive
            var ExcludedColumns = GetExcludedColumns().Select(c => c.ToLower()).ToHashSet();
            PropertyInfo[] properties = Dto.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!ExcludedColumns.Contains(property.Name.ToLower()))
                    cboSearchOptions.Items.Add(property.Name);
            }

            cboSearchOptions.SelectedIndex = 0;
        }

        protected void AddEditDeleteButtons()
        {
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "PresentationLayer", "Images", "EditIcon.png");
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "PresentationLayer", "Images", "DeleteIcon.png");

            //Resolve filepaths
            string editIconPath = Path.GetFullPath(filepath);
            string deleteIconPath = Path.GetFullPath(path);


            DataGridViewImageColumn editButtonColumn = new()
            {
                Name = "Edit",
                HeaderText = "",
                Image = Image.FromFile(editIconPath),  // Replace with your edit icon
                Width = 30,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            DataGridViewImageColumn deleteButtonColumn = new()
            {
                Name = "Delete",
                HeaderText = "",
                Image = Image.FromFile(deleteIconPath),  // Replace with your delete icon
                Width = 30,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            // Add Edit and Delete buttons
            dgvMain.Columns.Add(editButtonColumn);
            dgvMain.Columns.Add(deleteButtonColumn);

            // Prevent buttons from getting too large
            //   dgvMain.Columns["Edit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            //  dgvMain.Columns["Delete"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        }

        private void dgvMain_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dgvMain.Columns[e.ColumnIndex].Name == "Edit")
                {
                    btnEdit_Click(e.RowIndex);
                }
                else if (dgvMain.Columns[e.ColumnIndex].Name == "Delete")
                {
                    btnDelete_Click(e.RowIndex);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string SelectedOption = cboSearchOptions.SelectedItem.ToString();
            string SearchTerm = txtSearchBox.Text.Trim();
            ApplyFilter((DataTable)dgvMain.DataSource, SelectedOption, SearchTerm);
        }

        public bool isCaseSensitive = false; //Not case sensitive by default
        public void ApplyFilter(DataTable dataTable, string selectedOption, string searchTerm)
        {
            FormConsole.Instance.Log(searchTerm);

            if (dataTable == null || string.IsNullOrEmpty(selectedOption))
            {
                // Refresh dgv or show an error message
                FormConsole.Instance.Log("Invalid parameters for filtering.");
                return;
            }

            // null is a possible value for certain database tables. Do not filter it out
            if (string.IsNullOrEmpty(searchTerm) && searchTerm != null)
            {
                FormConsole.Instance.Log("Empty parameters for filtering.");
                return;
            }

            List<DataRow> filteredRows = FilterRows(dataTable, selectedOption, searchTerm, isCaseSensitive);

            DataTable filteredData = dataTable.Clone(); //Does not clone data, only the schema
            foreach (DataRow row in filteredRows)
            {
                filteredData.Rows.Add(row.ItemArray);  // Add the filtered rows directly to the DataTable
            }

            dgvMain.DataSource = filteredData;

            FormConsole.Instance.Log($"Filtered {filteredRows.Count} rows for '{selectedOption}' with search term '{searchTerm}' (CaseSensitive: {isCaseSensitive}).");
        }

        public static List<DataRow> FilterRows(DataTable dataTable, string selectedOption, string searchTerm, bool isCaseSensitive)
        {
            // Return an empty list if the search term is null/empty or if the column does not exist
            if (string.IsNullOrEmpty(searchTerm) || !dataTable.Columns.Contains(selectedOption))
                return new List<DataRow>();

            Type columnType = dataTable.Columns[selectedOption].DataType;

            if (columnType == typeof(int))
            {
                //Handle enum with case sensitivity
                if (Enum.TryParse(typeof(LicenseType), searchTerm, !isCaseSensitive, out var enumValue))
                {
                    int enumIntValue = (int)enumValue;
                    return dataTable.AsEnumerable()
                                    .Where(row => row.Field<int?>(selectedOption) == enumIntValue)
                                    .ToList();
                }
                return new List<DataRow>();
            }

            // Handle string columns with case sensitivity and partial matching
            if (columnType == typeof(string))
            {
                return dataTable.AsEnumerable()
                                .Where(row =>
                                {
                                    var fieldValue = row.Field<string>(selectedOption);
                                    if (fieldValue == null) return false;

                                    return isCaseSensitive
                                        ? fieldValue.Contains(searchTerm)
                                        : fieldValue.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
                                })
                                .ToList();
            }

            if (columnType == typeof(bool))
            {
                bool? boolSearchTerm = null;

                // Need to explicity look for accepted values else any unknown value resolves as true.
                if (searchTerm.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = true;
                }
                else if (searchTerm.Equals("0", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = false;
                }
                else if (searchTerm.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = true;
                }
                else if (searchTerm.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = false;
                }

                if (boolSearchTerm == null)
                {
                    return new List<DataRow>();
                }

                return dataTable.AsEnumerable()
                                .Where(row => row.Field<bool>(selectedOption) == boolSearchTerm)
                                .ToList();
            }

            return new List<DataRow>();
        }

        //Required by Children:
        protected virtual void btnAdd_Click(object sender, EventArgs e)
        {

        }

        protected virtual void btnEdit_Click(int RowIndex)
        {

        }

        protected virtual void btnDelete_Click(int RowIndex)
        {

        }

        protected virtual void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        protected virtual void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected virtual void rollbackToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected virtual void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

        }

        protected virtual void btnFirst_Click(object sender, EventArgs e)
        {

        }

        protected virtual void btnPrevious_Click(object sender, EventArgs e)
        {

        }

        protected virtual void btnNext_Click(object sender, EventArgs e)
        {

        }

        protected virtual void btnLast_Click(object sender, EventArgs e)
        {

        }

        protected virtual void btnGotoPage_Click(object sender, EventArgs e)
        {

        }

        protected virtual void btnPrint_Click(object sender, EventArgs e)
        {

        }

        private void txtSearchBox_Enter(object sender, EventArgs e)
        {
            if (txtSearchBox.Text == "Value for search")
            {
                txtSearchBox.Text = "";
                txtSearchBox.ForeColor = Color.Black;
            }
        }

        private void txtSearchBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchBox.Text))
            {
                txtSearchBox.Text = "Value for search";
                txtSearchBox.ForeColor = Color.Gray;
            }
        }


        private void txtStartPage_Enter(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() => (sender as System.Windows.Forms.TextBox).SelectAll()));
        }

        private void btnMatchCase_Click(object sender, EventArgs e)
        {
            isCaseSensitive = !isCaseSensitive;

            if (isCaseSensitive)
            {
                btnMatchCase.BackColor = Color.White;
            }
            else
            {
                btnMatchCase.BackColor = SoftBeige;
            }
        }

        //Required by xUnit Tests. Will only be usable during development

#if DEBUG
        public void OverrideDatagridView(DataTable table)
        {
            dgvMain.DataSource = table;
        }


        public DataTable GetDatagridViewTable()
        {
            return (DataTable)dgvMain.DataSource;
        }
#endif
    }
}
