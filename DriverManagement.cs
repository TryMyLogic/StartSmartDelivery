using SmartStartDeliveryForm.Classes;
using SmartStartDeliveryForm.DAOs;
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
        public DriverManagement()
        {
            InitializeComponent();
        }

        private void DriverManagement_Load(object sender, EventArgs e)
        {
            dataGridView1.RowHeadersVisible = false; // Hides Row Number Column
            
        

        // Create sample drivers
        Drivers driver1 = new Drivers
            {
                Name = "John",
                Surname = "Doe",
                EmployeeNo = 1,
                LicenseType = LicenseType.Code8,
                Availability = true
            };

            Drivers driver2 = new Drivers
            {
                Name = "Jane",
                Surname = "Smith",
                EmployeeNo = 2,
                LicenseType = LicenseType.Code10,
                Availability = false
            };

            Drivers driver3 = new Drivers
            {
                Name = "Alice",
                Surname = "Johnson",
                EmployeeNo = 3,
                LicenseType = LicenseType.Code14,
                Availability = true
            };

            // Add drivers to the DriversDAO
            //DriversDAO.DriverList.Add(driver1);
            //DriversDAO.DriverList.Add(driver2);
            //DriversDAO.DriverList.Add(driver3);

            // Clear any existing columns
            dataGridView1.Columns.Clear();

            // Create and configure columns
            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Name",
                DataPropertyName = "Name"
            };

            DataGridViewTextBoxColumn surnameColumn = new DataGridViewTextBoxColumn
            {
                Name = "Surname",
                HeaderText = "Surname",
                DataPropertyName = "Surname"
            };

            DataGridViewTextBoxColumn employeeNoColumn = new DataGridViewTextBoxColumn
            {
                Name = "EmployeeNo",
                HeaderText = "Employee No",
                DataPropertyName = "EmployeeNo"
            };

            DataGridViewTextBoxColumn licenseTypeColumn = new DataGridViewTextBoxColumn
            {
                Name = "LicenseType",
                HeaderText = "License Type",
                DataPropertyName = "LicenseType",
            };

            DataGridViewCheckBoxColumn availabilityColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Availability",
                HeaderText = "Availability",
                DataPropertyName = "Availability"
            };

            DataGridViewButtonColumn editButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = "",
                Text = "Edit",
                UseColumnTextForButtonValue = true
            };

            DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "",
                Text = "Delete",
                UseColumnTextForButtonValue = true
            };

            // Add columns to DataGridView
            dataGridView1.Columns.Add(nameColumn);
            dataGridView1.Columns.Add(surnameColumn);
            dataGridView1.Columns.Add(employeeNoColumn);
            dataGridView1.Columns.Add(licenseTypeColumn);
            dataGridView1.Columns.Add(availabilityColumn);
            dataGridView1.Columns.Add(editButtonColumn);
            dataGridView1.Columns.Add(deleteButtonColumn);

            SetTitle("Driver Management");
            // Bind DataSource to the DataGridView
            //dataGridView1.DataSource = DriversDAO.DriverList;
        }

        protected override void InsertBTN_Click(object sender, EventArgs e)
        {
            
            Drivers newDriver = new Drivers
            {
                Name = NameTXTBox.Text,
                Surname = SurnameTXTBox.Text,
                EmployeeNo = int.Parse(EmployeeNumberTXTBox.Text),
                LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), LicenseTypeCombobox.Text),
                Availability = true
            };

            //DriversDAO.DriverList.Add(newDriver);
        }
    }
}
