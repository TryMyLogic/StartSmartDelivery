using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public DriversDAO DriversDAO { get; private set; }
        public string ConnectionString { get; private set; }
        public DriverManagementForm DriverManagementForm { get; private set; }
        public DatabaseFixture()
        {
            IServiceProvider serviceRegistry = ServiceRegistry.RegisterServices("TestDB");
            ConnectionString = serviceRegistry.GetRequiredService<string>();
            DriversDAO = serviceRegistry.GetRequiredService<DriversDAO>();
            DriverManagementForm = serviceRegistry.GetRequiredService<DriverManagementForm>();
        }

        public void Dispose()
        {
           
        }
    }
}
