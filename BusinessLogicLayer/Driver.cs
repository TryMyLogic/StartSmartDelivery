using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    internal class Driver
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string EmployeeNo { get; set; }
        public required LicenseType LicenseType { get; set; }
        public required bool Availability { get; set; }

        public static bool IsEmployeeNoUnique(string EmployeeNo)
        {
            int Count = DriversDAO.GetEmployeeNoCount(EmployeeNo);
            bool IsUnique = Count == 0;
            FormConsole.Instance.Log($"Employee Unique: {IsUnique}");
            return IsUnique;
        }

    }
}
