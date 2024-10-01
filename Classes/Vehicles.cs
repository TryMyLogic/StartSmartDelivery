using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStartDeliveryForm.Classes
{
    internal class Vehicles
    {
        public string Make;
        public string Model;
        public int Year;
        public string NumberPlate;
        public int Availability; //Uses Enum
        public string VehicleAgeClass { get; private set; } 

        public Vehicles(string make, string model, int year, string numberPlate, int availability)
        {
            Make = make;
            Model = model;
            Year = year;
            NumberPlate = numberPlate;
            Availability = availability;
            CalculateVehicleAgeClass(); 
        }

        private void CalculateVehicleAgeClass()
        {
            int currentYear = DateTime.Now.Year;
            int vehicleAge = currentYear - Year;

            if (vehicleAge < 5)
            {
                VehicleAgeClass = "New";
            }
            else if (vehicleAge < 10)
            {
                VehicleAgeClass = "Moderately used";
            }
            else
            {
                VehicleAgeClass = "Old";
            }
        }
    }

}
