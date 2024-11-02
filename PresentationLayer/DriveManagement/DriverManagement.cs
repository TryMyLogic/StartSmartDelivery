using StartSmartDeliveryForm.Classes;
using StartSmartDeliveryForm.DAOs;
using StartSmartDeliveryForm.DataForms;
using StartSmartDeliveryForm.DTOs;
using StartSmartDeliveryForm.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StartSmartDeliveryForm
{
    public partial class DriverManagement : ManagementTemplate
    {
        private int _currentPage;
        private int _totalPages;
        private DataTable _driverData;
        public DriverManagement()
        {
            InitializeComponent();
        }

        private void DriverManagement_Load(object sender, EventArgs e)
        {
            dgvMain.RowHeadersVisible = false; // Hides Row Number Column

            // Clear any existing columns
            dgvMain.Columns.Clear();

            SetSearchOptions(typeof(DriversDTO));
            _driverData = DriversDAO.GetDriversAtPage(2);

            _currentPage = 1; // Always starts at page 1
            _totalPages = DriversDAO.GetTotalPages();
            lblStartEndPages.Text = $"{_currentPage}/{_totalPages}";

            if (_driverData == null || _driverData.Rows.Count == 0)
            {
                MessageBox.Show("Failed to load driver data. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dgvMain.DataSource = _driverData;
                AddEditDeleteButtons();

                // Hide the Primary Key Column from User
                dgvMain.Columns["DriverID"].Visible = false;
            }
        }

        protected override HashSet<string> GetExcludedColumns()
        {
            return new HashSet<string> { "DriverID" }; // By default, exclude nothing.
        }

        protected override void btnInsert_Click(object sender, EventArgs e)
        {
            DriverDataForm driverDataForm = new DriverDataForm();
            driverDataForm.SubmitClicked += DriverDataForm_SubmitClicked;
            driverDataForm.Show();
        }

        protected override void btnEdit_Click(int rowIndex)
        {
            DataGridViewRow selectedRow = dgvMain.Rows[rowIndex];

            DriversDTO driverData = new DriversDTO
            {
                DriverId = int.Parse(selectedRow.Cells["DriverID"].Value.ToString()),
                Name = selectedRow.Cells["Name"].Value.ToString(),
                Surname = selectedRow.Cells["Surname"].Value.ToString(),
                EmployeeNo = selectedRow.Cells["EmployeeNo"].Value.ToString(),
                LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), selectedRow.Cells["LicenseType"].Value.ToString()),
                Availability = bool.Parse(selectedRow.Cells["Availability"].Value.ToString())
            };

            DriverDataForm driverDataForm = new DriverDataForm
            {
                Mode = FormMode.Edit
            };

            driverDataForm.InitializeEditing(driverData);
            driverDataForm.SubmitClicked += DriverDataForm_SubmitClicked;
            driverDataForm.Show();
        }

        protected override void btnDelete_Click(int rowIndex)
        {
            DataGridViewRow selectedRow = dgvMain.Rows[rowIndex];
            int driverID = int.Parse(selectedRow.Cells["DriverID"].Value.ToString());
            DialogResult result = MessageBox.Show("Are you sure?", "Delete Row", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // Delete From Data Table
                _driverData.Rows.RemoveAt(rowIndex);

                //Delete From Database
                DriversDAO.DeleteDriver(driverID);
            }
        }

        // DriverDataForm submit button event handler
        private void DriverDataForm_SubmitClicked(object sender, EventArgs e)
        {
            if (sender is DriverDataForm form)
            {
                DriversDTO driverDTO = form.GetDriverData();

                if (form.Mode == FormMode.Add)
                {
                    int newDriverId = DriversDAO.InsertDriver(driverDTO);

                    if (newDriverId != -1) // Check for success
                    {
                        DataRow newRow = _driverData.NewRow();
                        newRow["DriverID"] = newDriverId;
                        newRow["Name"] = driverDTO.Name;
                        newRow["Surname"] = driverDTO.Surname;
                        newRow["EmployeeNo"] = driverDTO.EmployeeNo;
                        newRow["LicenseType"] = driverDTO.LicenseType;
                        newRow["Availability"] = driverDTO.Availability;

                        _driverData.Rows.Add(newRow);
                    }
                }
                else if (form.Mode == FormMode.Edit)
                {
                    DataRow rowToUpdate = _driverData.Rows.Find(driverDTO.DriverId); // Assuming EmployeeNo is the primary key

                    if (rowToUpdate != null)
                    {
                        rowToUpdate["Name"] = driverDTO.Name;
                        rowToUpdate["Surname"] = driverDTO.Surname;
                        rowToUpdate["EmployeeNo"] = driverDTO.EmployeeNo;
                        rowToUpdate["LicenseType"] = driverDTO.LicenseType;
                        rowToUpdate["Availability"] = driverDTO.Availability;
                    }

                    DriversDAO.UpdateDriver(driverDTO);
                    form.Close();
                }

                form.ClearData(); //Clear form for next batch of data
            }
        }

        protected override void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var dataTable = (DataTable)dgvMain.DataSource;

            // Remove any filters that were applied
            if (dataTable != null)
            {
                dataTable.DefaultView.RowFilter = string.Empty;  // Clear any applied filters
            }

            //Rebind
            dgvMain.DataSource = null;
            dgvMain.DataSource = _driverData;

            dgvMain.Columns["Name"].DisplayIndex = 0;
            dgvMain.Columns["Surname"].DisplayIndex = 1;
            dgvMain.Columns["EmployeeNo"].DisplayIndex = 2;
            dgvMain.Columns["LicenseType"].DisplayIndex = 3;
            dgvMain.Columns["Availability"].DisplayIndex = 4;
            dgvMain.Columns["Edit"].DisplayIndex = 5;
            dgvMain.Columns["Delete"].DisplayIndex = 6;

            // Hide the DriverID column
            dgvMain.Columns["DriverID"].Visible = false;

            MessageBox.Show("Succesfully Refreshed", "Refresh Status");
        }

        protected override void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Refetch data and Rebind
            _driverData = DriversDAO.GetAllDrivers();
            dgvMain.DataSource = null;
            dgvMain.DataSource = _driverData;

            dgvMain.Columns["Name"].DisplayIndex = 0;
            dgvMain.Columns["Surname"].DisplayIndex = 1;
            dgvMain.Columns["EmployeeNo"].DisplayIndex = 2;
            dgvMain.Columns["LicenseType"].DisplayIndex = 3;
            dgvMain.Columns["Availability"].DisplayIndex = 4;
            dgvMain.Columns["Edit"].DisplayIndex = 5;
            dgvMain.Columns["Delete"].DisplayIndex = 6;

            // Hide the DriverID column
            dgvMain.Columns["DriverID"].Visible = false;

            MessageBox.Show("Succesfully Reloaded", "Reload Status");
        }

        protected override void rollbackToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected override void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvMain.Columns[e.ColumnIndex].Name == "LicenseType" && e.Value != null)
            {
                try
                {
                    e.Value = ((LicenseType)e.Value).ToString();
                    e.FormattingApplied = true;
                }
                catch
                {
                    // Invalid Values are left as is (integers)
                    e.FormattingApplied = true;
                }
            }
        }

        protected override void btnFirst_Click(object sender, EventArgs e)
        {
            _currentPage = 1;
            SetPage(_currentPage);
        }

        protected override void btnPrevious_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                SetPage(_currentPage);
            }
        }

        protected override void btnNext_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                SetPage(_currentPage);
            }
        }

        protected override void btnLast_Click(object sender, EventArgs e)
        {
            _currentPage = _totalPages;
            SetPage(_currentPage);
        }

        private void SetPage(int currentPage)
        {
            lblStartEndPages.Text = $"{currentPage}/{_totalPages}";
            _driverData = DriversDAO.GetDriversAtPage(currentPage);
            dgvMain.DataSource = _driverData;
        }

        protected override void btnGotoPage_Click(object sender, EventArgs e)
        {
            bool ParsedGoto = int.TryParse(txtGotoPage.Text, out int GotoPage);
            if (ParsedGoto)
            {
                if (GotoPage == _currentPage) return; //Already on that page. Do nothing

                if (GotoPage >= 1 && GotoPage <= _totalPages)
                {
                    _currentPage = GotoPage;
                    SetPage(_currentPage);
                    txtGotoPage.Text = ""; 
                }
                else
                {
                    MessageBox.Show("GotoPage is out of range", "Invalid Number");
                    txtGotoPage.Text = "";
                    return;
                }
            }

        }
    }
}
