﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.DataLayer.Repositories;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public IRepository<DriversDTO> DriversRepository { get; private set; }
        public string ConnectionString { get; private set; }
        public bool CanConnectToDatabase { get; private set; }

        public DatabaseFixture()
        {
            IServiceProvider serviceRegistry = ServiceRegistry.RegisterServices("TestDB");
            ConnectionString = serviceRegistry.GetRequiredService<string>();
            DriversRepository = serviceRegistry.GetRequiredService<IRepository<DriversDTO>>();
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
