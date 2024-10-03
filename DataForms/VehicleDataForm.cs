using SmartStartDeliveryForm.Classes;
using SmartStartDeliveryForm.DTOs;
using SmartStartDeliveryForm.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartStartDeliveryForm.DataForms
{
    public partial class VehicleDataForm : Form
    {
        public int VehicleId { get; set; }
        public FormMode Mode { get; set; }
        public VehicleDataForm()
        {
            InitializeComponent();
        }

        private void VehicleDataForm_Load(object sender, EventArgs e)
        {
            cboAvailability.SelectedIndex = 0;
        }

        internal void InitializeEditing(VehiclesDTO VehicleData)
        {
            VehicleId = VehicleData.VehicleId;
            // Populate form with existing driver data for editing
            txtMake.Text = VehicleData.Make;
            txtModel.Text = VehicleData.Model;
            txtYear.Text = VehicleData.Year.ToString();
            txtNumberPlate.Text = VehicleData.NumberPlate;
            cboAvailability.SelectedItem = VehicleData.Availability.ToString();
        }

        private bool ValidForm()
        {

            //TODO

            return true;
        }

        public delegate void SubmitEventHandler(object sender, EventArgs e);
        public event SubmitEventHandler SubmitClicked;


        internal void ClearData()
        {
            txtMake.Clear();
            txtModel.Clear();
            txtYear.Clear();
            txtNumberPlate.Clear();
            cboAvailability.SelectedIndex = 0;
        }

        internal VehiclesDTO GetVehicleData()
        {
            VehiclesDTO Vehicle = new VehiclesDTO();
            Vehicle.VehicleId = this.VehicleId;
            Vehicle.Make = txtMake.Text;
            Vehicle.Model = txtModel.Text;
            if (!int.TryParse(txtYear.Text, out int year))
            {
                year = 2024; // Default value if parsing fails
            }
            Vehicle.Year = year;

            Vehicle.NumberPlate = txtNumberPlate.Text;
            Vehicle.Availability = (int)(VehicleAvailabilityEnum)Enum.Parse(typeof(VehicleAvailabilityEnum), cboAvailability.SelectedItem.ToString());

            return Vehicle;
        }

        public void SubmitBTN_Click(object sender, EventArgs e)
        {
            if (ValidForm())
            {
                SubmitClicked?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
