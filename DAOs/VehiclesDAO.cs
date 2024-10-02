using SmartStartDeliveryForm.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStartDeliveryForm.DAOs
{
    internal class VehiclesDAO
    {
      

        internal static DataTable UpdateVehicleNonDatabase(VehiclesDTO vehicleDTO, DataTable vehicleData)
        {
            try
            {
                // Find the row in the DataTable with the matching VehicleID
                DataRow[] rowsToUpdate = vehicleData.Select($"VehicleID = {vehicleDTO.VehicleId}");

                if (rowsToUpdate.Length > 0)
                {
                    DataRow row = rowsToUpdate[0];

                    // Update the relevant fields
                    row["Make"] = vehicleDTO.Make;
                    row["Model"] = vehicleDTO.Model;
                    row["Year"] = vehicleDTO.Year;
                    row["NumberPlate"] = vehicleDTO.NumberPlate;
                    row["Availability"] = vehicleDTO.Availability;

                    // Simulate logging the success
                    FormConsole.Instance.Log($"Vehicle with ID: {vehicleDTO.VehicleId} updated successfully.");
                }
                else
                {
                    FormConsole.Instance.Log($"Vehicle with ID: {vehicleDTO.VehicleId} not found.");
                }

                return vehicleData; // Return the updated DataTable
            }
            catch (Exception ex)
            {
                // Simulate logging the error
                FormConsole.Instance.Log("An error occurred while updating the vehicle: " + ex.Message);
                return null; // Indicates an error occurred
            }
        }

    }
}
