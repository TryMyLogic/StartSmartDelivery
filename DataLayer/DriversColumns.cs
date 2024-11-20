using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm.DataLayer
{
    // Prevents introducing typos. Also improves maintainability by allow a string change in one location instead of multiple
    internal class DriverColumns
    {
        public const string DriverID = "DriverID";
        public const string Name = "Name";
        public const string Surname = "Surname";
        public const string EmployeeNo = "EmployeeNo";
        public const string LicenseType = "LicenseType";
        public const string Availability = "Availability";

        public const string Edit = "Edit";
        public const string Delete = "Delete";
    }
}
