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

            DataTable dgvDataTable = (DataTable)dgvMain.DataSource;
            ApplyFilter(dgvDataTable, SelectedOption, SearchTerm);
        }

        public static void ApplyFilter(DataTable dataTable, string selectedOption, string searchTerm)
        {
            if (dataTable == null || string.IsNullOrEmpty(selectedOption))
            {
                FormConsole.Instance.Log("Invalid parameters for filtering.");
                return;
            }

            //TODO - Get input from matchcase button. By Default, search is not case sensitive
            bool isCaseSensitive = false;
            string filterExpression = BuildFilterExpression(dataTable, selectedOption, searchTerm);

            if (!string.IsNullOrEmpty(filterExpression))
            {
                dataTable.DefaultView.RowFilter = filterExpression;
                FormConsole.Instance.Log($"Filter applied: {filterExpression}");
            }
            else
            {
                dataTable.DefaultView.RowFilter = string.Empty;  // Reset filter if expression is empty
                FormConsole.Instance.Log("Filter removed.");
            }
        }

        public static string BuildFilterExpression(DataTable dataTable, string selectedOption, string searchTerm)
        {
            string filterExpression = "";

            if (string.IsNullOrEmpty(searchTerm))
            {
                return filterExpression;
            }

            if (!dataTable.Columns.Contains(selectedOption))
            {
                return filterExpression;
            }

            searchTerm = searchTerm.Replace("'", "''");
            Type columnType = dataTable.Columns[selectedOption].DataType;

            if (columnType == typeof(string))
            {
                filterExpression = $"{selectedOption} LIKE '%{searchTerm}%'";
            }
            else if (columnType == typeof(int))
            {
                filterExpression = SearchEnumColumn(selectedOption, searchTerm);

                if (filterExpression != string.Empty)
                {
                    return filterExpression; // Enum value found, return immediately
                }

                if (int.TryParse(searchTerm, out int intSearchTerm))
                {
                    filterExpression = $"{selectedOption} = {intSearchTerm}";
                }
                else
                {
                    filterExpression = "";
                }
            }
            else if (columnType == typeof(bool))
            {
                if (bool.TryParse(searchTerm, out bool boolSearchTerm))
                {
                    filterExpression = $"{selectedOption} = {boolSearchTerm.ToString().ToLower()}";
                }
                else if (searchTerm == "1")
                {
                    filterExpression = $"{selectedOption} = true";
                }
                else if (searchTerm == "0")
                {
                    filterExpression = $"{selectedOption} = false";
                }
            }
            return filterExpression;
        }

        /*
        Note: The SearchEnum function works on the assumption that
        1. The namespace for Enums is the same across the board
        2. The column name for any Enum column have the same name as the 
           Enum itself.
         */
        public static string SearchEnumColumn(string selectedOption, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return string.Empty;
            }

            Type? EnumType = Type.GetType($"StartSmartDeliveryForm.SharedLayer.Enums.{selectedOption}", false);
            if (EnumType == null || !EnumType.IsEnum)
            {
                return string.Empty;
            }
            FormConsole.Instance.Log("enumType" + EnumType);

            string[] enumNames = Enum.GetNames(EnumType);
            bool isValidEnum = enumNames.Any(name =>
                string.Equals(name, searchTerm, StringComparison.OrdinalIgnoreCase)
            );

            if (!isValidEnum)
            {
                FormConsole.Instance.Log($"Invalid {selectedOption} search term.");
                return string.Empty;
            }

            try
            {
                object enumValue = Enum.Parse(EnumType, searchTerm, true); // Always case-insensitive
                return $"{selectedOption} = {(int)enumValue}";
            }
            catch (ArgumentException)
            {
                FormConsole.Instance.Log($"Invalid {selectedOption} search term.");
                return string.Empty;
            }
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

        protected virtual void refreshToolStripMenuItem_Click(object sender, EventArgs e)
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


    }
}
