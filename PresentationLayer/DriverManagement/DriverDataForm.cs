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
using StartSmartDeliveryForm.SharedLayer;
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

        private void DriverDataForm_Load(object sender, EventArgs e)
        {
            cboAvailability.SelectedIndex = 0;  //Is true by default
            btnSubmit.BackColor = GlobalConstants.SoftBeige;
            btnSubmit.FlatAppearance.BorderSize = 0;
        }

        internal void InitializeEditing(DriversDTO DriverData)
        {
            DriverId = DriverData.DriverID;
            // Populate form with existing driver data for editing
            txtName.Text = DriverData.Name;
            txtSurname.Text = DriverData.Surname;
            txtEmployeeNo.Text = DriverData.EmployeeNo;
            cboLicenseType.SelectedItem = DriverData.LicenseType.ToString();
            cboAvailability.SelectedItem = DriverData.Availability.ToString();
        }

        private bool ValidForm()
        {
            if (!DataFormValidator.IsValidString(txtName.Text, "Name")) return false;

            if (!DataFormValidator.IsValidString(txtSurname.Text, "Surname")) return false;

            if (!DataFormValidator.IsValidString(txtEmployeeNo.Text, "EmployeeNo"))
            {
                // Only check uniqueness if the field isn't empty && Mode is not edit
                if (Mode == FormMode.Add && !Driver.IsEmployeeNoUnique(txtEmployeeNo.Text))
                {
                    MessageBox.Show("Employee No is not unique.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            if (!DataFormValidator.IsValidEnumValue<LicenseType>(cboLicenseType.Text, "License Type")) return false;
            if (!DataFormValidator.IsValidBoolValue(cboAvailability.Text, "Availability")) return false;

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
