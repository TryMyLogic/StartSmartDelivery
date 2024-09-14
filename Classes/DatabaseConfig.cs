using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStartDeliveryForm.Classes
{
     internal class DatabaseConfig
    {
        static string _ConnectionString;
        public static void Initialize(string serverAddress, string databaseName)
        {
            // Use Windows Authentication (Trusted Connection)
            _ConnectionString = $"Server={serverAddress};Database={databaseName};Trusted_Connection=True;";
        }

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_ConnectionString))
                {
                    throw new InvalidOperationException("Connection string not initialized. Call Initialize() first.");
                }
                return _ConnectionString;
            }
        }
    }
}
