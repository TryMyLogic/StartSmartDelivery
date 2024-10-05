using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace StartSmartDeliveryForm.Classes
{

    internal class DatabaseConfig
    {
        static string _ConnectionString;

        // Initialize the connection string from app.config
        public static void Initialize(string ConnectionStringName)
        {
            // Retrieve the connection string from app.config
            _ConnectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString;

            if (string.IsNullOrEmpty(_ConnectionString))
            {
                throw new InvalidOperationException("Connection string not found in configuration file.");
            }
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
