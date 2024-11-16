using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    public partial class DriverManagementForm : ManagementTemplateForm
    {
        private int _currentPage;
        private int _totalPages;
        private DataTable _driverData;
        int _recordsCount = DriversDAO.GetRecordCount();
        public DriverManagementForm()
        {
            InitializeComponent();
            _driverData = new DataTable(); //Empty table by default
        }

        private void DriverManagementForm_Load(object sender, EventArgs e)
        {
            AdjustDataGridViewHeight(dgvMain);
            SetSearchOptions(typeof(DriversDTO));
            _driverData = DriversDAO.GetDriversAtPage(1) ?? new DataTable();

            _currentPage = 1; // Always starts at page 1
            _totalPages = (int)Math.Ceiling((double)_recordsCount / GlobalConstants.s_recordLimit);

            txtStartPage.Text = $"{_currentPage}";
            lblEndPage.Text = $"/{_totalPages}";

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
            return ["DriverID"]; // By default, exclude nothing.
        }

        protected override void btnAdd_Click(object sender, EventArgs e)
        {
            DriverDataForm driverDataForm = new();
            driverDataForm.SubmitClicked += DriverDataForm_SubmitClicked;
            driverDataForm.Show();
        }

        protected override void btnEdit_Click(int rowIndex)
        {
            DataGridViewRow selectedRow = dgvMain.Rows[rowIndex];

            object DriverID = selectedRow.Cells["DriverID"].Value;
            object Name = selectedRow.Cells["Name"].Value;
            object Surname = selectedRow.Cells["Surname"].Value;
            object EmployeeNo = selectedRow.Cells["EmployeeNo"].Value;
            object LicenseType = selectedRow.Cells["LicenseType"].Value;
            object Availability = selectedRow.Cells["Availability"].Value;

            if (DriverID != null &&
                Name != null &&
                Surname != null &&
                EmployeeNo != null &&
                LicenseType != null &&
                Availability != null &&
                int.TryParse(DriverID.ToString(), out int driverID) &&
                Enum.TryParse(LicenseType.ToString(), out LicenseType license) &&
                bool.TryParse(Availability.ToString(), out bool availability))
            {
                DriversDTO driverData = new(
                    driverID,
                    Name.ToString()!,
                    Surname.ToString()!,
                    EmployeeNo.ToString()!,
                    license,
                    availability
                );

                DriverDataForm driverDataForm = new()
                {
                    Mode = FormMode.Edit
                };

                driverDataForm.InitializeEditing(driverData);
                driverDataForm.SubmitClicked += DriverDataForm_SubmitClicked;
                driverDataForm.Show();
            }
            else
            {
                MessageBox.Show("An error occurred. Edit form could not be initialized due to invalid data.");
            }
        }


        protected override void btnDelete_Click(int rowIndex)
        {
            DataGridViewRow selectedRow = dgvMain.Rows[rowIndex];

            object DriverID = selectedRow.Cells["DriverID"].Value;

            DialogResult result = MessageBox.Show("Are you sure?", "Delete Row", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes && int.TryParse(DriverID.ToString(), out int driverID))
            {
                _driverData.Rows.RemoveAt(rowIndex);
                DriversDAO.DeleteDriver(driverID);

                _recordsCount--;
                _totalPages = (int)Math.Ceiling((double)_recordsCount / GlobalConstants.s_recordLimit);
                if (_currentPage > _totalPages)
                {
                    _currentPage = _totalPages;
                    SetPage(_currentPage);
                }
            }
        }

        // DriverDataForm submit button event handler
        private void DriverDataForm_SubmitClicked(object sender, EventArgs e)
        {
            if (sender is DriverDataForm form)
            {
                DriversDTO driverDTO = form.GetDriverData();

                if (driverDTO != null)
                {
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

                            _recordsCount++;
                            _totalPages = (int)Math.Ceiling((double)_recordsCount / GlobalConstants.s_recordLimit);
                            if (_currentPage < _totalPages)
                            {
                                _currentPage = _totalPages;
                                SetPage(_currentPage);
                            }

                        }
                    }
                    else if (form.Mode == FormMode.Edit)
                    {
                        DataRow? rowToUpdate = _driverData.Rows.Find(driverDTO.DriverId); // Assuming EmployeeNo is the primary key

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
                }

                form.ClearData(); //Clear form for next batch of data
            }
        }

        protected override void btnRefresh_Click(object sender, EventArgs e)
        {
            var dataTable = (DataTable)dgvMain.DataSource;

            // Remove any filters that were applied
            if (dataTable != null)
            {
                dataTable.DefaultView.RowFilter = string.Empty;  // Clear any applied filters
            }

            // Rebind
            dgvMain.DataSource = null;
            dgvMain.DataSource = _driverData;

            dgvMain.Columns["Name"].DisplayIndex = 0;
            dgvMain.Columns["Surname"].DisplayIndex = 1;
            dgvMain.Columns["EmployeeNo"].DisplayIndex = 2;
            dgvMain.Columns["LicenseType"].DisplayIndex = 3;
            dgvMain.Columns["Availability"].DisplayIndex = 4;
            dgvMain.Columns["Edit"].DisplayIndex = 5;
            dgvMain.Columns["Delete"].DisplayIndex = 6;

            dgvMain.Columns["DriverID"].Visible = false;

            MessageBox.Show("Succesfully Refreshed", "Refresh Status");
        }

        protected override void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Refetch data and rebind
            _driverData = DriversDAO.GetAllDrivers() ?? new DataTable();
            dgvMain.DataSource = null;
            dgvMain.DataSource = _driverData;

            dgvMain.Columns["Name"].DisplayIndex = 0;
            dgvMain.Columns["Surname"].DisplayIndex = 1;
            dgvMain.Columns["EmployeeNo"].DisplayIndex = 2;
            dgvMain.Columns["LicenseType"].DisplayIndex = 3;
            dgvMain.Columns["Availability"].DisplayIndex = 4;
            dgvMain.Columns["Edit"].DisplayIndex = 5;
            dgvMain.Columns["Delete"].DisplayIndex = 6;

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
            txtStartPage.Text = $"{_currentPage}";
            lblEndPage.Text = $"/{_totalPages}";

            _driverData = DriversDAO.GetDriversAtPage(currentPage) ?? new DataTable();
            dgvMain.DataSource = _driverData;
        }

        protected override void btnGotoPage_Click(object sender, EventArgs e)
        {
            bool ParsedGoto = int.TryParse(txtStartPage.Text, out int GotoPage);
            if (ParsedGoto)
            {
                if (GotoPage == _currentPage) return;

                if (GotoPage >= 1 && GotoPage <= _totalPages)
                {
                    _currentPage = GotoPage;
                    SetPage(_currentPage);
                    txtStartPage.Text = $"{_currentPage}";
                }
                else
                {
                    MessageBox.Show("GotoPage is out of range", "Invalid Number");
                    txtStartPage.Text = $"{_currentPage}";
                    return;
                }
            }
        }

    }
}
