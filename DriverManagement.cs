using SmartStartDeliveryForm.Classes;
using SmartStartDeliveryForm.DAOs;
using SmartStartDeliveryForm.DataForms;
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

namespace SmartStartDeliveryForm
{
    public partial class DriverManagement : ManagementTemplate
    {
        private DataTable DriverData;
        public DriverManagement()
        {
            InitializeComponent();
        }

        private void DriverManagement_Load(object sender, EventArgs e)
        {
            dataGridView1.RowHeadersVisible = false; // Hides Row Number Column

            // Clear any existing columns
            dataGridView1.Columns.Clear();

            DataGridViewButtonColumn EditButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = "",
                Text = "Edit",
                UseColumnTextForButtonValue = true
            };

            DataGridViewButtonColumn DeleteButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "",
                Text = "Delete",
                UseColumnTextForButtonValue = true
            };

            SetTitle("Driver Management");
            SetSearchOptions(typeof(DriversDTO));

            DriverData = DriversDAO.GetAllDrivers();
            dataGridView1.DataSource = DriverData;

            // Add columns to DataGridView
            dataGridView1.Columns.Add(EditButtonColumn);
            dataGridView1.Columns.Add(DeleteButtonColumn);
        }

        protected override void InsertBTN_Click(object sender, EventArgs e)
        {

            DriverDataForm DriverDataForm = new DriverDataForm();
            DriverDataForm.SubmitClicked += DriverDataForm_SubmitClicked;
            DriverDataForm.Show();
        }

        // Submit button click event handler
        private void DriverDataForm_SubmitClicked(object sender, EventArgs e)
        {
            // Handle the event
            DriverDataForm Form = sender as DriverDataForm;
            if (Form != null)
            {
                DriversDTO DriverDTO = Form.GetDriverData();

                if (Form.Mode == FormMode.Add)
                {
                    DriversDAO.InsertDriver(DriverDTO);
                }
                else if (Form.Mode == FormMode.Edit)
                {
                    DriversDAO.UpdateDriver(DriverDTO); // Ensure you have an UpdateDriver method
                }

                Form.ClearData(); //Clear form for next batch of data
            }
        }

        protected override void EditBTN_Click(int RowIndex)
        {
            var SelectedRow = dataGridView1.Rows[RowIndex];

            DriversDTO DriverData = new DriversDTO
            {
                Name = SelectedRow.Cells["Name"].Value.ToString(),
                Surname = SelectedRow.Cells["Surname"].Value.ToString(),
                EmployeeNo = SelectedRow.Cells["EmployeeNo"].Value.ToString(),
                LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), SelectedRow.Cells["LicenseType"].Value.ToString()),
                Availability = bool.Parse(SelectedRow.Cells["Availability"].Value.ToString())
            };

            DriverDataForm driverDataForm = new DriverDataForm
            {
                Mode = FormMode.Edit
            };

            driverDataForm.InitializeEditing(DriverData);
            driverDataForm.SubmitClicked += DriverDataForm_SubmitClicked;
            driverDataForm.Show();
        }
        protected override void DeleteBTN_Click(int RowIndex)
        {
            var SelectedRow = dataGridView1.Rows[RowIndex];
            string EmployeeNo = SelectedRow.Cells["EmployeeNo"].Value.ToString();
            DialogResult Result = MessageBox.Show("Are you sure?", "Delete Row", MessageBoxButtons.YesNo);
            if (Result == DialogResult.Yes)
            {
                // Delete From Data Table
                DriverData.Rows.RemoveAt(RowIndex);

                //Delete From Database
                DriversDAO.DeleteDriver(EmployeeNo);
            }
        }
    }
}
