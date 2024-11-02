using StartSmartDeliveryForm.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.DAOs;

namespace StartSmartDeliveryForm.Classes
{
    internal class Drivers
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string EmployeeNo { get; set; }
        public LicenseType LicenseType { get; set; }
        public bool Availability { get; set; }

        public static bool IsEmployeeNoUnique(string EmployeeNo)
        {
            int Count = DriversDAO.GetEmployeeNoCount(EmployeeNo);
            bool IsUnique = Count == 0;
            FormConsole.Instance.Log($"Employee Unique: {IsUnique}");
            return IsUnique;
        }

    }
}
