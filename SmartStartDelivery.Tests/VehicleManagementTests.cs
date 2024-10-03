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

namespace SmartStartDelivery.Tests
{
    public class VehicleManagementTests
    {
        [Theory]
        [InlineData("Toyota", "Prius", 2023, "NUM123GP", 1)] //Test passes
        [InlineData("Honda", "Civic", 2020, "NUM543GP", 2)] //Test passes
        [InlineData("Ford", "F-150", 2021, "NUM645GP", 3)] //Test fails - found bug since 3 is valid (stands for Vehicle under Maintenance)
        public void VehicleDataForm_SubmitClicked_AddsVehicle_WhenInAddMode(string Make, string Model, int Year, string NumberPlate,int Availability)
        {
            // Arrange 

            //===== VehicleManagement.cs =====
            //Each instance of the form has a unique datatable (non-static)
            //In actual function, VehiclesDAO.GetAllVehicles() or VehiclesFake.GetSampleVehicles
            //would be called on form load to populate Datatable using database/fake sample data. 
            VehicleManagement VehicleManagementForm = new VehicleManagement();
           
            var TestVehicleData = new DataTable();
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

        //[Fact]
        //public void VehicleDataForm_SubmitClicked_UpdatesVehicle_WhenInEditMode()
        //{
        //    // Arrange
        //    var mockVehicleDTO = new VehiclesDTO
        //    {
        //        VehicleId = 1,
        //        Make = "Updated Make",
        //        Model = "Updated Model",
        //        Year = 2024,
        //        NumberPlate = "XYZ789",
        //        Availability = 2
        //    };

        //    var mockVehicleDataForm = new VehicleDataForm
        //    {
        //        Mode = FormMode.Edit
        //    };

        //    var vehicleData = new DataTable();
        //    vehicleData.Columns.Add("VehicleID", typeof(int));
        //    vehicleData.Columns.Add("Make", typeof(string));
        //    vehicleData.Columns.Add("Model", typeof(string));
        //    vehicleData.Columns.Add("Year", typeof(int));
        //    vehicleData.Columns.Add("NumberPlate", typeof(string));
        //    vehicleData.Columns.Add("Availability", typeof(int));

        //    DataRow row = vehicleData.NewRow();
        //    row["VehicleID"] = 1;
        //    row["Make"] = "Old Make";
        //    row["Model"] = "Old Model";
        //    row["Year"] = 2020;
        //    row["NumberPlate"] = "OLD456";
        //    row["Availability"] = 1;
        //    vehicleData.Rows.Add(row);

        //    var form = new VehicleDataForm();

        //    // Act
        //    form.SubmitClicked += (sender, e) =>
        //    {
        //        DataRow rowToUpdate = vehicleData.Rows.Find(mockVehicleDTO.VehicleId);
        //        if (rowToUpdate != null)
        //        {
        //            rowToUpdate["Make"] = mockVehicleDTO.Make;
        //            rowToUpdate["Model"] = mockVehicleDTO.Model;
        //            rowToUpdate["Year"] = mockVehicleDTO.Year;
        //            rowToUpdate["NumberPlate"] = mockVehicleDTO.NumberPlate;
        //            rowToUpdate["Availability"] = mockVehicleDTO.Availability;
        //        }
        //    };

        //    // Trigger form submission event
        //    form.SubmitBTN_Click(this, EventArgs.Empty);

        //    // Assert
        //    Assert.Equal("Updated Make", vehicleData.Rows[0]["Make"]);
        //    Assert.Equal("Updated Model", vehicleData.Rows[0]["Model"]);
        //    Assert.Equal(2024, vehicleData.Rows[0]["Year"]);
       // }

        //[Fact]
        //public void DeleteBTN_Click_RemovesRow_FromDataTable()
        //{
        //    // Arrange
        //    var vehicleData = new DataTable();
        //    vehicleData.Columns.Add("VehicleID", typeof(int));
        //    vehicleData.Columns.Add("Make", typeof(string));
        //    vehicleData.Columns.Add("Model", typeof(string));
        //    vehicleData.Columns.Add("Year", typeof(int));
        //    vehicleData.Columns.Add("NumberPlate", typeof(string));
        //    vehicleData.Columns.Add("Availability", typeof(int));

        //    DataRow row = vehicleData.NewRow();
        //    row["VehicleID"] = 1;
        //    row["Make"] = "Test Make";
        //    row["Model"] = "Test Model";
        //    row["Year"] = 2023;
        //    row["NumberPlate"] = "ABC123";
        //    row["Availability"] = 1;
        //    vehicleData.Rows.Add(row);

        //    // Act
        //    var rowIndex = 0; // Simulate row index
        //    vehicleData.Rows.RemoveAt(rowIndex);

        //    // Assert
        //    Assert.Equal(0, vehicleData.Rows.Count); // Ensure row is removed
        //}

    }
}
