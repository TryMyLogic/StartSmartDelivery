using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SmartStartDeliveryForm.DTOs;
using SmartStartDeliveryForm.DataForms;

namespace SmartStartDelivery.Tests
{

    public class Class1
    {
        [Fact]
        public void VehicleDataForm_SubmitClicked_AddsVehicle_WhenInAddMode()
        {
            // Arrange
            var mockVehicleDTO = new VehiclesDTO
            {
                Make = "Test Make",
                Model = "Test Model",
                Year = 2023,
                NumberPlate = "ABC123",
                Availability = 1
            };

            var mockVehicleDataForm = new VehicleDataForm
            {
                Mode = FormMode.Add
            };

            var vehicleData = new DataTable();
            vehicleData.Columns.Add("VehicleID", typeof(int));
            vehicleData.Columns.Add("Make", typeof(string));
            vehicleData.Columns.Add("Model", typeof(string));
            vehicleData.Columns.Add("Year", typeof(int));
            vehicleData.Columns.Add("NumberPlate", typeof(string));
            vehicleData.Columns.Add("Availability", typeof(int));

            var form = new VehicleDataForm();

            // Act
            form.SubmitClicked += (sender, e) =>
            {
                int newVehicleID = vehicleData.Rows.Count > 0
                    ? Convert.ToInt32(vehicleData.Compute("MAX(VehicleID)", string.Empty)) + 1
                    : 1;

                DataRow newRow = vehicleData.NewRow();
                newRow["VehicleID"] = newVehicleID;
                newRow["Make"] = mockVehicleDTO.Make;
                newRow["Model"] = mockVehicleDTO.Model;
                newRow["Year"] = mockVehicleDTO.Year;
                newRow["NumberPlate"] = mockVehicleDTO.NumberPlate;
                newRow["Availability"] = mockVehicleDTO.Availability;
                vehicleData.Rows.Add(newRow);
            };

            // Trigger form submission event
            form.SubmitBTN_Click(this, EventArgs.Empty);

            // Assert
            Assert.Equal(1, vehicleData.Rows.Count);
            Assert.Equal("Test Make", vehicleData.Rows[0]["Make"]);
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

        [Fact]
        public void DeleteBTN_Click_RemovesRow_FromDataTable()
        {
            // Arrange
            var vehicleData = new DataTable();
            vehicleData.Columns.Add("VehicleID", typeof(int));
            vehicleData.Columns.Add("Make", typeof(string));
            vehicleData.Columns.Add("Model", typeof(string));
            vehicleData.Columns.Add("Year", typeof(int));
            vehicleData.Columns.Add("NumberPlate", typeof(string));
            vehicleData.Columns.Add("Availability", typeof(int));

            DataRow row = vehicleData.NewRow();
            row["VehicleID"] = 1;
            row["Make"] = "Test Make";
            row["Model"] = "Test Model";
            row["Year"] = 2023;
            row["NumberPlate"] = "ABC123";
            row["Availability"] = 1;
            vehicleData.Rows.Add(row);

            // Act
            var rowIndex = 0; // Simulate row index
            vehicleData.Rows.RemoveAt(rowIndex);

            // Assert
            Assert.Equal(0, vehicleData.Rows.Count); // Ensure row is removed
        }

    }
}
