using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{

    internal partial class DriverDataForm : DataFormTemplate, IDriverDataForm
    {
        private int _driverID;
        public DriverDataForm(ILogger<DriverDataForm>? logger = null, IMessageBox? messageBox = null) : base(logger, messageBox)
        {
            InitializeComponent();
        }

        private void DriverDataForm_Load(object sender, EventArgs e)
        {
            cboAvailability.SelectedIndex = 0;  //Is true by default
        }

        int IDriverDataForm.DriverID
        {
            get => _driverID;
            set => _driverID = value;
        }

        string IDriverDataForm.Name
        {
            get => txtName.Text;
            set => txtName.Text = value;
        }

        string IDriverDataForm.Surname
        {
            get => txtSurname.Text;
            set => txtSurname.Text = value;
        }

        string IDriverDataForm.EmployeeNo
        {
            get => txtEmployeeNo.Text;
            set => txtEmployeeNo.Text = value;
        }

        LicenseType IDriverDataForm.LicenseType
        {
            get => cboLicenseType.SelectedItem != null
                ? (LicenseType)Enum.Parse(typeof(LicenseType), cboLicenseType.SelectedItem.ToString()!)
                : default;

            set => cboLicenseType.SelectedItem = value.ToString();
        }

        bool IDriverDataForm.Availability
        {
            get => bool.Parse(cboAvailability.SelectedItem!.ToString()!);
            set => cboAvailability.SelectedItem = value.ToString();
        }

        public override void InitializeEditing(object DriverData)
        {
            // Populate form with existing driver data for editing
            DriversDTO driverData = (DriversDTO)DriverData;
            _driverID = driverData.DriverID;
            txtName.Text = driverData.Name;
            txtSurname.Text = driverData.Surname;
            txtEmployeeNo.Text = driverData.EmployeeNo;
            cboLicenseType.SelectedItem = driverData.LicenseType.ToString();
            cboAvailability.SelectedItem = driverData.Availability.ToString();
        }

        public override void ClearData()
        {
            txtName.Clear();
            txtSurname.Clear();
            txtEmployeeNo.Clear();
            cboLicenseType.SelectedIndex = -1;
            cboAvailability.SelectedIndex = 0;
        }

        public override DriversDTO GetData()
        {
            //Valid form ensures data here is never null and can be succefully parsed
            return new DriversDTO(
                _driverID,
                txtName.Text,
                txtSurname.Text,
                txtEmployeeNo.Text,
                (LicenseType)Enum.Parse(typeof(LicenseType), cboLicenseType.SelectedItem!.ToString()!),
                bool.Parse(cboAvailability.SelectedItem!.ToString()!)
            );
        }
    }
}
