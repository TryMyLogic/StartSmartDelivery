using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using Serilog;

namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    internal class Driver(DriversDAO driversDAO)
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string EmployeeNo { get; set; }
        public LicenseType LicenseType { get; set; }
        public bool Availability { get; set; }

        public CancellationTokenSource Cts { get; private set; }

        public async Task<bool> IsEmployeeNoUnique(string EmployeeNo)
        {
            Cts = new CancellationTokenSource();

            try
            {
                int count = await driversDAO.GetEmployeeNoCountAsync(EmployeeNo, Cts.Token);
                bool isUnique = count == 0;
                Log.Information($"Employee Unique: {isUnique}");
                return isUnique;
            }
            catch (OperationCanceledException)
            {
                Log.Information("IsEmployeeNoUnique was cancelled");
                return false;
            }

        }
    }

}
