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
        public int DriverId { get; }
        public string Name { get; }
        public string Surname { get; }
        public string EmployeeNo { get; }
        public LicenseType LicenseType { get; }
        public bool Availability { get; }

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
        public DriversDTO(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
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
