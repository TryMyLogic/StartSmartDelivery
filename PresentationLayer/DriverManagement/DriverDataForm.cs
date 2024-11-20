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
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{

    public partial class DriverDataForm : DataFormTemplate
    {
        public int DriverID { get; set; }

        public DriverDataForm()
        {
            InitializeComponent();
        }

        private void DriverDataForm_Load(object sender, EventArgs e)
        {
            cboAvailability.SelectedIndex = 0;  //Is true by default
        }

        protected override bool ValidForm()
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

        internal override void InitializeEditing(object DriverData)
        {
            DriversDTO driverData = (DriversDTO)DriverData;
            DriverID = driverData.DriverID;
            // Populate form with existing driver data for editing
            txtName.Text = driverData.Name;
            txtSurname.Text = driverData.Surname;
            txtEmployeeNo.Text = driverData.EmployeeNo;
            cboLicenseType.SelectedItem = driverData.LicenseType.ToString();
            cboAvailability.SelectedItem = driverData.Availability.ToString();
        }

        internal override void ClearData()
        {
            txtName.Clear();
            txtSurname.Clear();
            txtEmployeeNo.Clear();
            cboLicenseType.SelectedIndex = -1;
            cboAvailability.SelectedIndex = 0;
        }

        internal override DriversDTO GetData()
        {
            //Valid form ensures data here is never null and can be succefully parsed
            return new DriversDTO(
                DriverID,
                txtName.Text,
                txtSurname.Text,
                txtEmployeeNo.Text,
                (LicenseType)Enum.Parse(typeof(LicenseType), cboLicenseType.SelectedItem!.ToString()!),
                bool.Parse(cboAvailability.SelectedItem!.ToString()!)
            );
        }
    }
}
