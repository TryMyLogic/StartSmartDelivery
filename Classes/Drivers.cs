using SmartStartDeliveryForm.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStartDeliveryForm.Classes
{
    internal class Drivers
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int EmployeeNo { get; set; }
        public LicenseType LicenseType { get; set; } 
        public bool Availability { get; set; }

    }
}
