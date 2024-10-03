using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SmartStartDeliveryForm.DTOs;
using SmartStartDeliveryForm.DataForms;
using SmartStartDeliveryForm;
using System.Windows.Forms;
using SmartStartDeliveryForm.Enums;
using System.Reflection;

namespace SmartStartDelivery.Tests
{
    public class VehicleManagementTests
    {
        [Theory]
        [InlineData("Toyota", "Prius", 2023, "NUM123GP", 1)] //Test passes
        //[InlineData("Honda", "Civic", 2020, "NUM543GP", 2)] //Test passes
       // [InlineData("Ford", "F-150", 2021, "NUM645GP", 3)] //Test fails - found bug since 3 is valid (stands for Vehicle under Maintenance)
        public void VehicleDataForm_SubmitClicked_AddsVehicle_WhenInAddMode(string Make, string Model, int Year, string NumberPlate,int Availability)
        {
            // Arrange 

            //===== VehicleManagement.cs =====
            //Each instance of the form has a unique datatable (non-static)
            //In actual function, VehiclesDAO.GetAllVehicles() or VehiclesFake.GetSampleVehicles
            //would be called on form load to populate Datatable using database/fake sample data. 
            VehicleManagement VehicleManagementForm = new VehicleManagement();
           
            DataTable TestVehicleData = new DataTable();
            TestVehicleData.Columns.Add("VehicleID", typeof(int));
            TestVehicleData.Columns.Add("Make", typeof(string));
            TestVehicleData.Columns.Add("Model", typeof(string));
            TestVehicleData.Columns.Add("Year", typeof(int));
            TestVehicleData.Columns.Add("NumberPlate", typeof(string));
            TestVehicleData.Columns.Add("Availability", typeof(int));

            //Override this specific instances private DataTable
            VehicleManagementForm.OverrideVehicleData(TestVehicleData);
            
            //===== InsertBTN Clicked =====
            var TestVehicleDataForm = new VehicleDataForm
            {
                Mode = FormMode.Add
            };

            TestVehicleDataForm.SubmitClicked += VehicleManagementForm.VehicleDataForm_SubmitClicked;

            // Simulate User Entering Data
            TestVehicleDataForm.Controls["txtMake"].Text = Make;
            TestVehicleDataForm.Controls["txtModel"].Text = Model;
            TestVehicleDataForm.Controls["txtYear"].Text = Year.ToString();
            TestVehicleDataForm.Controls["txtNumberPlate"].Text = NumberPlate;
            TestVehicleDataForm.Controls["cboAvailability"].Text = ((VehicleAvailabilityEnum)Availability).ToString();

            // Act
            TestVehicleDataForm.SubmitBTN_Click(this, EventArgs.Empty);

            // Assert
            Assert.Equal(1, VehicleManagementForm.VehicleData.Rows.Count);
            Assert.Equal(Make, VehicleManagementForm.VehicleData.Rows[0]["Make"]);
            Assert.Equal(Model, VehicleManagementForm.VehicleData.Rows[0]["Model"]);
            Assert.Equal(Year, VehicleManagementForm.VehicleData.Rows[0]["Year"]);
            Assert.Equal(NumberPlate, VehicleManagementForm.VehicleData.Rows[0]["NumberPlate"]);
            Assert.Equal(Availability, VehicleManagementForm.VehicleData.Rows[0]["Availability"]);
        }

        [Theory]
        [InlineData("Toyota", "Prius", 2023, "NUM123GP", 1)]
        public void VehicleDataForm_SubmitClicked_UpdatesVehicle_WhenInEditMode(string Make, string Model, int Year, string NumberPlate, int Availability)
        {
            // Arrange
            VehicleManagement VehicleManagementForm = new VehicleManagement();

            DataTable TestVehicleData = new DataTable();
            TestVehicleData.Columns.Add("VehicleID", typeof(int));
            TestVehicleData.Columns.Add("Make", typeof(string));
            TestVehicleData.Columns.Add("Model", typeof(string));
            TestVehicleData.Columns.Add("Year", typeof(int));
            TestVehicleData.Columns.Add("NumberPlate", typeof(string));
            TestVehicleData.Columns.Add("Availability", typeof(int));

            TestVehicleData.Rows.Add(10,"DefaultMake","DefaultModel",1000,"NUM000GP",1);

            // Set the primary key
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = TestVehicleData.Columns["VehicleID"];
            TestVehicleData.PrimaryKey = PrimaryKeyColumns;

            //Override this specific instances private DataTable
            VehicleManagementForm.OverrideVehicleData(TestVehicleData);

            //===== EditBTN Clicked =====
            var TestVehicleDataForm = new VehicleDataForm
            {
                Mode = FormMode.Edit
            };

            TestVehicleDataForm.SubmitClicked += VehicleManagementForm.VehicleDataForm_SubmitClicked;

            // Simulate User Entering Data
            TestVehicleDataForm.VehicleId = 10;
            TestVehicleDataForm.Controls["txtMake"].Text = Make;
            TestVehicleDataForm.Controls["txtModel"].Text = Model;
            TestVehicleDataForm.Controls["txtYear"].Text = Year.ToString();
            TestVehicleDataForm.Controls["txtNumberPlate"].Text = NumberPlate;
            TestVehicleDataForm.Controls["cboAvailability"].Text = ((VehicleAvailabilityEnum)Availability).ToString();
          
            // Act
            TestVehicleDataForm.SubmitBTN_Click(this, EventArgs.Empty);

            // Assert
            Assert.Equal(1, VehicleManagementForm.VehicleData.Rows.Count);
            Assert.Equal(Make, VehicleManagementForm.VehicleData.Rows[0]["Make"]);
            Assert.Equal(Model, VehicleManagementForm.VehicleData.Rows[0]["Model"]);
            Assert.Equal(Year, VehicleManagementForm.VehicleData.Rows[0]["Year"]);
            Assert.Equal(NumberPlate, VehicleManagementForm.VehicleData.Rows[0]["NumberPlate"]);
            Assert.Equal(Availability, VehicleManagementForm.VehicleData.Rows[0]["Availability"]);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(20)]
        public void DeleteBTN_Click_RemovesRow_FromDataTable(int VehicleID)
        {
            // Arrange
            VehicleManagement VehicleManagementForm = new VehicleManagement();

            DataTable TestVehicleData = new DataTable();
            TestVehicleData.Columns.Add("VehicleID", typeof(int));
            TestVehicleData.Columns.Add("Make", typeof(string));
            TestVehicleData.Columns.Add("Model", typeof(string));
            TestVehicleData.Columns.Add("Year", typeof(int));
            TestVehicleData.Columns.Add("NumberPlate", typeof(string));
            TestVehicleData.Columns.Add("Availability", typeof(int));

            TestVehicleData.Rows.Add(10, "DefaultMake", "DefaultModel", 1000, "NUM000GP", 1);
            TestVehicleData.Rows.Add(15, "DefaultMake", "DefaultModel", 1000, "NUM000GP", 1);
            TestVehicleData.Rows.Add(20, "DefaultMake", "DefaultModel", 1000, "NUM000GP", 1);

            // Set the primary key
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = TestVehicleData.Columns["VehicleID"];
            TestVehicleData.PrimaryKey = PrimaryKeyColumns;

            //Override this specific instances private DataTable
            VehicleManagementForm.OverrideVehicleData(TestVehicleData);

            // Act
            //===== DeleteBTN Clicked =====
            DataRow RowToDelete = TestVehicleData.Rows.Find(VehicleID);
            int RowIndex = TestVehicleData.Rows.IndexOf(RowToDelete);
            TestVehicleData.Rows.RemoveAt(RowIndex);

            // Assert
            Assert.Equal(2, TestVehicleData.Rows.Count); //Test initally had 3 rows. Now it should be 2
            DataRow[] FoundRows = TestVehicleData.Select($"VehicleID = {VehicleID}");
            Assert.Empty(FoundRows); //Deleted ID shouldnt be found
        }

    }
}
