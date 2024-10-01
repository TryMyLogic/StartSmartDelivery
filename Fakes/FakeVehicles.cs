using SmartStartDeliveryForm.Classes;
using System.Collections.Generic;
using System.Data;

namespace SmartStartDeliveryForm.Fakes
{
    internal class FakeVehicles
    {
        public static DataTable GetSampleVehicles()
        {
            DataTable Dt = new DataTable();
            Dt.Columns.Add("VehicleID", typeof(int));
            Dt.Columns.Add("Make", typeof(string));
            Dt.Columns.Add("Model", typeof(string));
            Dt.Columns.Add("Year", typeof(int));
            Dt.Columns.Add("NumberPlate", typeof(string));
            Dt.Columns.Add("Availability", typeof(int));
            Dt.Columns.Add("VehicleAgeClass", typeof(string));

            List<Vehicles> vehicleList = new List<Vehicles>
{
    new Vehicles("Toyota", "Camry", 2020, "ABC123", 1),
    new Vehicles("Honda", "Civic", 2019, "XYZ789", 1),
    new Vehicles("Ford", "Focus", 2018, "LMN456", 2),
    new Vehicles("Chevrolet", "Malibu", 2021, "DEF321", 1),
    new Vehicles("Nissan", "Altima", 2017, "GHI654", 2),
    new Vehicles("Hyundai", "Elantra", 2020, "JKL987", 1),
    new Vehicles("Kia", "Optima", 2022, "MNO159", 1),
    new Vehicles("Subaru", "Impreza", 2016, "PQR753", 3),
    new Vehicles("Volkswagen", "Jetta", 2015, "STU258", 2),
    new Vehicles("Mazda", "3", 2023, "VWX369", 1)
};

            int vehicleID = 1; // Start vehicle ID from 1
            foreach (var vehicle in vehicleList)
            {
                Dt.Rows.Add(vehicleID++, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.NumberPlate, vehicle.Availability, vehicle.VehicleAgeClass);
            }

            // Set the primary key
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = Dt.Columns["VehicleID"];
            Dt.PrimaryKey = PrimaryKeyColumns;

            return Dt;
        }
    }
}
