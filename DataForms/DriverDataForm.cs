using SmartStartDeliveryForm.DTOs;
using SmartStartDeliveryForm.Enums;
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
using SmartStartDeliveryForm.Classes;

namespace SmartStartDeliveryForm.DataForms
{
    public enum FormMode
    {
        Add,
        Edit
    }
    public partial class DriverDataForm : Form
    {
        public int DriverId { get; set; }
        public FormMode Mode { get; set; }
        public DriverDataForm()
        {
            InitializeComponent();
        }

        private void DriverDataForm_Load(object sender, EventArgs e)
        {
            cboAvailability.SelectedIndex = 0;  //Is true by default
        }

        internal void InitializeEditing(DriversDTO DriverData)
        {
            DriverId = DriverData.DriverId;
            // Populate form with existing driver data for editing
            txtName.Text = DriverData.Name;
            txtSurname.Text = DriverData.Surname;
            txtEmployeeNo.Text = DriverData.EmployeeNo;
            cboLicenseType.SelectedItem = DriverData.LicenseType.ToString();
            cboAvailability.SelectedItem = DriverData.Availability.ToString();
        }

        private bool ValidForm()
        {

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name field cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSurname.Text))
            {
                MessageBox.Show("Surname field cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmployeeNo.Text))
            {
                MessageBox.Show("Employee No field cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                // Only check uniqueness if the field isnt empty && Mode is not edit
                if (this.Mode == FormMode.Add && !Drivers.IsEmployeeNoUnique(txtEmployeeNo.Text))
                {
                    MessageBox.Show("Employee No is not unique.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

            }

            if (!Enum.TryParse(cboLicenseType.Text, out LicenseType licenseType))
            {
                MessageBox.Show("Please select a valid License Type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!bool.TryParse(cboAvailability.Text, out bool availability))
            {
                MessageBox.Show("Availability must be 'True' or 'False'.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        public delegate void SubmitEventHandler(object sender, EventArgs e);
        public event SubmitEventHandler SubmitClicked;
        private void SubmitBTN_Click(object sender, EventArgs e)
        {
            if (ValidForm())
            {
                SubmitClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        internal void ClearData()
        {
            txtName.Clear();
            txtSurname.Clear();
            txtEmployeeNo.Clear();
            cboLicenseType.SelectedIndex = -1;
            cboAvailability.SelectedIndex = 0;
        }
        internal DriversDTO GetDriverData()
        {
            DriversDTO Driver = new DriversDTO();
            Driver.DriverId = this.DriverId;
            Driver.Name = txtName.Text;
            Driver.Surname = txtSurname.Text;
            Driver.EmployeeNo = txtEmployeeNo.Text;
            Driver.LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), cboLicenseType.SelectedItem.ToString());
            Driver.Availability = bool.Parse(cboAvailability.SelectedItem.ToString());

            return Driver;
        }
    }
}
