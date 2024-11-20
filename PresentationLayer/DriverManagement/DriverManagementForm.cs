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
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    public partial class DriverManagementForm : ManagementTemplateForm
    {

        private DataTable _driverData;
        private readonly PaginationManager _paginationManager;
        public DriverManagementForm()
        {
            InitializeComponent();
            _driverData = new DataTable(); //Empty table by default

            _paginationManager = new PaginationManager("Drivers");
            _paginationManager.PageChanged += OnPageChanged;
        }

        public void OnPageChanged(int currentPage)
        {
            _driverData = DriversDAO.GetDriversAtPage(currentPage) ?? new DataTable();
            dgvMain.DataSource = _driverData;
            txtStartPage.Text = $"{_paginationManager.CurrentPage}";
            lblEndPage.Text = $"/{_paginationManager.TotalPages}";
        }

        private void DriverManagementForm_Load(object sender, EventArgs e)
        {
            AdjustDataGridViewHeight(dgvMain);
            SetSearchOptions(typeof(DriversDTO));
            _paginationManager.GoToFirstPage();

            if (_driverData == null || _driverData.Rows.Count == 0)
            {
                MessageBox.Show("Failed to load driver data. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dgvMain.DataSource = _driverData;
                AddEditDeleteButtons();

                // Hide the Primary Key Column from User
                DataGridViewColumn? driverColumn = dgvMain.Columns[DriverColumns.DriverID];
                if (driverColumn != null)
                {
                    driverColumn.Visible = false;
                }
            }
        }

        protected override HashSet<string> GetExcludedColumns()
        {
            return [DriverColumns.DriverID]; // By default, exclude nothing.
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

            object? DriverID = selectedRow.Cells[DriverColumns.DriverID].Value;
            object? Name = selectedRow.Cells[DriverColumns.Name].Value;
            object? Surname = selectedRow.Cells[DriverColumns.Surname].Value;
            object? EmployeeNo = selectedRow.Cells[DriverColumns.EmployeeNo].Value;
            object? LicenseType = selectedRow.Cells[DriverColumns.LicenseType].Value;
            object? Availability = selectedRow.Cells[DriverColumns.Availability].Value;

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

            object? DriverID = selectedRow.Cells[DriverColumns.DriverID].Value;

            DialogResult result = MessageBox.Show("Are you sure?", "Delete Row", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes && int.TryParse(DriverID?.ToString(), out int driverID))
            {
                _driverData.Rows.RemoveAt(rowIndex);
                DriversDAO.DeleteDriver(driverID);

                _paginationManager.UpdateRecordCount(_paginationManager.RecordCount - 1);
                _paginationManager.EnsureValidPage();

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
                            newRow[DriverColumns.DriverID] = newDriverId;
                            newRow[DriverColumns.Name] = driverDTO.Name;
                            newRow[DriverColumns.Surname] = driverDTO.Surname;
                            newRow[DriverColumns.EmployeeNo] = driverDTO.EmployeeNo;
                            newRow[DriverColumns.LicenseType] = driverDTO.LicenseType;
                            newRow[DriverColumns.Availability] = driverDTO.Availability;

                            _driverData.Rows.Add(newRow);

                            _paginationManager.UpdateRecordCount(_paginationManager.RecordCount + 1);
                            _paginationManager.GoToLastPage(); // Allows user to see successful insert

                        }
                    }
                    else if (form.Mode == FormMode.Edit)
                    {
                        DataRow? rowToUpdate = _driverData.Rows.Find(driverDTO.DriverID); // Assuming EmployeeNo is the primary key

                        if (rowToUpdate != null)
                        {
                            rowToUpdate[DriverColumns.Name] = driverDTO.Name;
                            rowToUpdate[DriverColumns.Surname] = driverDTO.Surname;
                            rowToUpdate[DriverColumns.EmployeeNo] = driverDTO.EmployeeNo;
                            rowToUpdate[DriverColumns.LicenseType] = driverDTO.LicenseType;
                            rowToUpdate[DriverColumns.Availability] = driverDTO.Availability;
                        }

                        DriversDAO.UpdateDriver(driverDTO);
                        form.Close();
                    }
                }

                form.ClearData(); //Clear form for next batch of data
            }
        }

        private void SetDataGridViewColumns()
        {
            DataGridViewColumn? nameCol = dgvMain.Columns[DriverColumns.Name];
            DataGridViewColumn? surnameCol = dgvMain.Columns[DriverColumns.Surname];
            DataGridViewColumn? employeeNoCol = dgvMain.Columns[DriverColumns.EmployeeNo];
            DataGridViewColumn? licenseTypeCol = dgvMain.Columns[DriverColumns.LicenseType];
            DataGridViewColumn? availabilityCol = dgvMain.Columns[DriverColumns.Availability];
            DataGridViewColumn? editCol = dgvMain.Columns[DriverColumns.Edit];
            DataGridViewColumn? deleteCol = dgvMain.Columns[DriverColumns.Delete];
            DataGridViewColumn? driverIDCol = dgvMain.Columns[DriverColumns.DriverID];

            if (nameCol != null && surnameCol != null && employeeNoCol != null &&
                licenseTypeCol != null && availabilityCol != null &&
                editCol != null && deleteCol != null && driverIDCol != null)
            {
                nameCol.DisplayIndex = 0;
                surnameCol.DisplayIndex = 1;
                employeeNoCol.DisplayIndex = 2;
                licenseTypeCol.DisplayIndex = 3;
                availabilityCol.DisplayIndex = 4;
                editCol.DisplayIndex = 5;
                deleteCol.DisplayIndex = 6;

                driverIDCol.Visible = false;
            }
        }

        protected override void btnRefresh_Click(object sender, EventArgs e)
        {
            DataTable? dataTable = null;
            if (dgvMain.DataSource is DataTable dt)
            {
                dataTable = dt;
            }

            // Remove any filters that were applied
            if (dataTable != null)
            {
                dataTable.DefaultView.RowFilter = string.Empty;  // Clear any applied filters
            }

            // Rebind
            dgvMain.DataSource = null;
            dgvMain.DataSource = _driverData;
            SetDataGridViewColumns();

            MessageBox.Show("Succesfully Refreshed", "Refresh Status");
        }

        protected override void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Refetch data and rebind
            _driverData = DriversDAO.GetAllDrivers() ?? new DataTable();
            dgvMain.DataSource = null;
            dgvMain.DataSource = _driverData;
            SetDataGridViewColumns();

            MessageBox.Show("Succesfully Reloaded", "Reload Status");
        }

        protected override void rollbackToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected override void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvMain.Columns[e.ColumnIndex].Name == DriverColumns.LicenseType && e.Value != null)
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
            _paginationManager.GoToFirstPage();
        }

        protected override void btnPrevious_Click(object sender, EventArgs e)
        {
            _paginationManager.GoToPreviousPage();
        }

        protected override void btnNext_Click(object sender, EventArgs e)
        {
            _paginationManager.GoToNextPage();
        }

        protected override void btnLast_Click(object sender, EventArgs e)
        {
            _paginationManager.GoToLastPage();
        }

        protected override void btnGotoPage_Click(object sender, EventArgs e)
        {
            bool ParsedGoto = int.TryParse(txtStartPage.Text, out int GotoPage);
            if (ParsedGoto)
            {

                if (GotoPage == _paginationManager.CurrentPage) return;

                if (GotoPage >= 1 && GotoPage <= _paginationManager.TotalPages)
                {
                    _paginationManager.GoToPage(GotoPage);
                }
                else
                {
                    MessageBox.Show("Page number is out of range", "Invalid Number");
                    return;
                }
            }
        }

        protected override void btnPrint_Click(object sender, EventArgs e)
        {
            PrintDriverDataForm preview = new(dgvMain); // Prints only 1 page. 

            //Unlike Show, it blocks execution on main form till complete
            preview.ShowDialog();
        }
    }
}
