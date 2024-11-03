using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.DataLayer.DTOs
{
    /*
    DTO:
    Encapsulates data to be transferred between different layers of an application (e.g., from data access to business logic).
    Reduces the number of method calls by bundling multiple data attributes into a single object.
    Improves performance by minimizing the amount of data that needs to be serialized/deserialized during communication.
    Promotes a clear separation between the data structure and business logic, enhancing maintainability and clarity.
    */

    internal class DriversDTO
    {
        public int DriverId { get; }
        public string Name { get; }
        public string Surname { get; }
        public string EmployeeNo { get; }
        public LicenseType LicenseType { get; }
        public bool Availability { get; }

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
            DriverId = DriverID;
            this.Name = Name;
            this.Surname = Surname;
            this.EmployeeNo = EmployeeNo;
            this.LicenseType = LicenseType;
            this.Availability = Availability;
        }
    }
}
