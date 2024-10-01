using SmartStartDeliveryForm.DAOs;
using SmartStartDeliveryForm.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartStartDeliveryForm.Fakes;
using SmartStartDeliveryForm.DataForms;

namespace SmartStartDeliveryForm
{
    public partial class VehicleManagement : ManagementTemplate
    {
        private DataTable VehicleData;
        public VehicleManagement()
        {
            InitializeComponent();
        }

        private void VehicleManagement_Load(object sender, EventArgs e)
        {
            dataGridView1.RowHeadersVisible = false; // Hides Row Number Column

            // Clear any existing columns
            dataGridView1.Columns.Clear();

            SetSearchOptions(typeof(VehiclesDTO));
            VehicleData = FakeVehicles.GetSampleVehicles();

            if (VehicleData == null || VehicleData.Rows.Count == 0)
            {
                MessageBox.Show("Failed to load driver data. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dataGridView1.DataSource = VehicleData;

                DataGridViewButtonColumn EditButtonColumn = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                };

                DataGridViewButtonColumn DeleteButtonColumn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                };

                // Hide the VehicleID column
                dataGridView1.Columns["VehicleID"].Visible = false;

                // Add Edit and Delete buttons
                dataGridView1.Columns.Add(EditButtonColumn);
                dataGridView1.Columns.Add(DeleteButtonColumn);

                // Prevent buttons from getting too large
                dataGridView1.Columns["Edit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["Delete"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
        }

        protected override void InsertBTN_Click(object sender, EventArgs e)
        {

          //  DriverDataForm diverDataForm = new DriverDataForm();
           // DriverDataForm.SubmitClicked += DriverDataForm_SubmitClicked;
           // DriverDataForm.Show();
        }

    }
}
