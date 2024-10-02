using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStartDeliveryForm.DTOs
{
    public class VehiclesDTO
    {
        public int VehicleId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string NumberPlate { get; set; }
        public int Availability { get; set; }
    }
}
