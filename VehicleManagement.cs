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
using SmartStartDeliveryForm.Classes;
using SmartStartDeliveryForm.Enums;

namespace SmartStartDeliveryForm
{
    public partial class VehicleManagement : ManagementTemplate
    {
        public DataTable VehicleData;
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

        public void OverrideVehicleData(DataTable dataTable)
        {
            VehicleData = dataTable;
            dataGridView1.DataSource = VehicleData;
        }

        protected override void InsertBTN_Click(object sender, EventArgs e)
        {

            VehicleDataForm VehicleDataForm = new VehicleDataForm();
            VehicleDataForm.SubmitClicked += VehicleDataForm_SubmitClicked;
            VehicleDataForm.Show();
        }

        // Submit button click event handler
        public void VehicleDataForm_SubmitClicked(object sender, EventArgs e)
        {
            // Handle the event
            VehicleDataForm Form = sender as VehicleDataForm;
            if (Form != null)
            {
                VehiclesDTO VehicleDTO = Form.GetVehicleData();
                FormConsole.Instance.Log("Mode: " + Form.Mode);
                if (Form.Mode == FormMode.Add)
                {

                    int newVehicleID = VehicleData.Rows.Count > 0 ?
                     Convert.ToInt32(VehicleData.Compute("MAX(VehicleID)", string.Empty)) + 1 :
                     1;

                    if (newVehicleID != -1) // Check for success
                    {
                        DataRow NewRow = VehicleData.NewRow();
                        NewRow["VehicleID"] = newVehicleID;
                        NewRow["Make"] = VehicleDTO.Make;
                        NewRow["Model"] = VehicleDTO.Model;
                        NewRow["Year"] = VehicleDTO.Year;
                        NewRow["NumberPlate"] = VehicleDTO.NumberPlate;
                        NewRow["Availability"] = VehicleDTO.Availability;

                        // Add to DataTable
                        VehicleData.Rows.Add(NewRow);
                    }
                }
                else if (Form.Mode == FormMode.Edit)
                {
                    DataRow RowToUpdate = VehicleData.Rows.Find(VehicleDTO.VehicleId); // Assuming EmployeeNo is the primary key

                    if (RowToUpdate != null)
                    {
                        RowToUpdate["Make"] = VehicleDTO.Make;
                        RowToUpdate["Model"] = VehicleDTO.Model;
                        RowToUpdate["Year"] = VehicleDTO.Year;
                        RowToUpdate["NumberPlate"] = VehicleDTO.NumberPlate;
                        RowToUpdate["Availability"] = VehicleDTO.Availability;
                    }

                    Form.Close();
                }

                Form.ClearData(); //Clear form for next batch of data
            }
        }

        protected override void EditBTN_Click(int RowIndex)
        {
            var SelectedRow = dataGridView1.Rows[RowIndex];

            VehiclesDTO VehicleData = new VehiclesDTO
            {
                VehicleId = int.Parse(SelectedRow.Cells["VehicleID"].Value.ToString()),
                Make = SelectedRow.Cells["Make"].Value.ToString(),
                Model = SelectedRow.Cells["Model"].Value.ToString(),
                Year = int.Parse(SelectedRow.Cells["Year"].Value.ToString()),
                NumberPlate = SelectedRow.Cells["NumberPlate"].Value.ToString(),
                Availability = int.Parse(SelectedRow.Cells["Availability"].Value.ToString())
            };

            VehicleDataForm VehicleDataForm = new VehicleDataForm
            {
                Mode = FormMode.Edit
            };

            VehicleDataForm.InitializeEditing(VehicleData);
            VehicleDataForm.SubmitClicked += VehicleDataForm_SubmitClicked;
            VehicleDataForm.Show();
        }

        protected override void DeleteBTN_Click(int RowIndex)
        {
            var SelectedRow = dataGridView1.Rows[RowIndex];
            int VehicleID = int.Parse(SelectedRow.Cells["VehicleID"].Value.ToString());
            DialogResult Result = MessageBox.Show("Are you sure?", "Delete Row", MessageBoxButtons.YesNo);
            if (Result == DialogResult.Yes)
            {
                // Delete From Data Table
                VehicleData.Rows.RemoveAt(RowIndex);

                //Delete From Database
               // VehiclesDAO.DeleteDriver(DriverID);
            }
        }

    }
}
