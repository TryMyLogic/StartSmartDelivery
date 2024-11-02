using StartSmartDeliveryForm.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm.DTOs
{
    /*
     * DTO's 
     */
    internal class DriversDTO
    {
        public int DriverId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string EmployeeNo { get; set; }
        public LicenseType LicenseType { get; set; }
        public bool Availability { get; set; }

        public DriversDTO() { }

        public DriversDTO(string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            //DriverID is 0 by default when unassigned
            this.Name = Name;
            this.Surname = Surname;
            this.EmployeeNo = EmployeeNo;
            this.LicenseType = LicenseType;
            this.Availability = Availability;
        }
        public DriversDTO(int DriverID,string Name, string Surname, string EmployeeNo, LicenseType LicenseType,bool Availability)
        {
            this.DriverId = DriverID;
            this.Name = Name;
            this.Surname = Surname;
            this.EmployeeNo = EmployeeNo;
            this.LicenseType = LicenseType;
            this.Availability = Availability;
        }
    }
}
