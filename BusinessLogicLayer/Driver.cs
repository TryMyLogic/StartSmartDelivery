using Serilog;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    internal class Driver(DriversDAO driversDAO)
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string EmployeeNo { get; set; }
        public LicenseType LicenseType { get; set; }
        public bool Availability { get; set; }

        public async Task<bool> IsEmployeeNoUnique(string EmployeeNo)
        {
            int count = await driversDAO.GetEmployeeNoCountAsync(EmployeeNo);
            bool isUnique = count == 0;
            Log.Information($"Employee Unique: {isUnique}");
            return isUnique;
        }
    }

}
