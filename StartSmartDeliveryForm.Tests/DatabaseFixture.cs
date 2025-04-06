using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.Generics;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public IRepository<DriversDTO> DriversRepository { get; private set; }
        public DriversDAO DriversDAO { get; private set; }
        public string ConnectionString { get; private set; }
        public DriverManagementForm DriverManagementForm { get; private set; }
        public bool CanConnectToDatabase { get; private set; }

        public DatabaseFixture()
        {
            IServiceProvider serviceRegistry = ServiceRegistry.RegisterServices("TestDB");
            ConnectionString = serviceRegistry.GetRequiredService<string>();
            DriversDAO = serviceRegistry.GetRequiredService<DriversDAO>();
            DriversRepository = serviceRegistry.GetRequiredService<IRepository<DriversDTO>>(); 
            DriverManagementForm = serviceRegistry.GetRequiredService<DriverManagementForm>();
            CanConnectToDatabase = TestConnectionToDB(ConnectionString);
        }

        private static bool TestConnectionToDB(string ConString)
        {
            try
            {
                using (SqlConnection Connection = new(ConString))
                {
                    Connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {

        }
    }
}
