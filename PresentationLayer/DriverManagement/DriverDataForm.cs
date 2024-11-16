using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
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

        public static readonly Color SoftBeige = Color.FromArgb(240, 221, 188);
        private void DriverDataForm_Load(object sender, EventArgs e)
        {
            cboAvailability.SelectedIndex = 0;  //Is true by default
            btnSubmit.BackColor = SoftBeige;
            btnSubmit.FlatAppearance.BorderSize = 0;
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
                if (Mode == FormMode.Add && !Driver.IsEmployeeNoUnique(txtEmployeeNo.Text))
                {
                    MessageBox.Show("Employee No is not unique.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

            }

            if (!Enum.TryParse<LicenseType>(cboLicenseType.Text, out _))
            {
                MessageBox.Show("Please select a valid License Type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!bool.TryParse(cboAvailability.Text, out _))
            {
                MessageBox.Show("Availability must be 'True' or 'False'.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        public delegate void SubmitEventHandler(object sender, EventArgs e);
        public event SubmitEventHandler? SubmitClicked; 
        private void btnSubmit_Click(object sender, EventArgs e)
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
            //Valid form ensures data here is never null and can be succefully parsed
            return new DriversDTO(
                DriverId,
                txtName.Text,
                txtSurname.Text,
                txtEmployeeNo.Text,
                (LicenseType)Enum.Parse(typeof(LicenseType), cboLicenseType.SelectedItem!.ToString()!),
                bool.Parse(cboAvailability.SelectedItem!.ToString()!)
            );
        }
    }
}
