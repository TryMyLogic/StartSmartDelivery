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
                UseColumnTextForButtonValue = true,
             
            };

            DataGridViewButtonColumn DeleteButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "",
                Text = "Delete",
                UseColumnTextForButtonValue = true,

             
            };

          
            //SetTitle("Driver Management");
            SetSearchOptions(typeof(DriversDTO));

            DriverData = DriversDAO.GetAllDrivers();
            dataGridView1.DataSource = DriverData;

            // Add columns to DataGridView
            dataGridView1.Columns.Add(EditButtonColumn);
            dataGridView1.Columns.Add(DeleteButtonColumn);

            dataGridView1.Columns["Edit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["Delete"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
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
                    DataRow NewRow = DriverData.NewRow();
                    NewRow["Name"] = DriverDTO.Name;
                    NewRow["Surname"] = DriverDTO.Surname;
                    NewRow["EmployeeNo"] = DriverDTO.EmployeeNo;
                    NewRow["LicenseType"] = DriverDTO.LicenseType; // Assuming LicenseType is an int
                    NewRow["Availability"] = DriverDTO.Availability;

                    //Add to datatable
                    DriverData.Rows.Add(NewRow);

                    //Add to database
                    DriversDAO.InsertDriver(DriverDTO);
                }
                else if (Form.Mode == FormMode.Edit)
                {
                    DataRow RowToUpdate = DriverData.Rows.Find(DriverDTO.EmployeeNo); // Assuming EmployeeNo is the primary key
                    if (RowToUpdate != null)
                    {
                        RowToUpdate["Name"] = DriverDTO.Name;
                        RowToUpdate["Surname"] = DriverDTO.Surname;
                        RowToUpdate["EmployeeNo"] = DriverDTO.EmployeeNo;
                        RowToUpdate["LicenseType"] = DriverDTO.LicenseType;
                        RowToUpdate["Availability"] = DriverDTO.Availability;
                    }

                    DriversDAO.UpdateDriver(DriverDTO);
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

        protected override void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Rebind
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = DriverData;

            dataGridView1.Columns["Name"].DisplayIndex = 0;
            dataGridView1.Columns["Surname"].DisplayIndex = 1;
            dataGridView1.Columns["EmployeeNo"].DisplayIndex = 2;
            dataGridView1.Columns["LicenseType"].DisplayIndex = 3;
            dataGridView1.Columns["Availability"].DisplayIndex = 4;
            dataGridView1.Columns["Edit"].DisplayIndex = 5;
            dataGridView1.Columns["Delete"].DisplayIndex = 6;

            MessageBox.Show("Succesfully Refreshed", "Refresh Status");
        }

        protected override void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Refetch data and Rebind
            DriverData = DriversDAO.GetAllDrivers();
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = DriverData;

            dataGridView1.Columns["Name"].DisplayIndex = 0;
            dataGridView1.Columns["Surname"].DisplayIndex = 1;
            dataGridView1.Columns["EmployeeNo"].DisplayIndex = 2;
            dataGridView1.Columns["LicenseType"].DisplayIndex = 3;
            dataGridView1.Columns["Availability"].DisplayIndex = 4;
            dataGridView1.Columns["Edit"].DisplayIndex = 5;
            dataGridView1.Columns["Delete"].DisplayIndex = 6;

            MessageBox.Show("Succesfully Reloaded", "Reload Status");
        }
    }
}
