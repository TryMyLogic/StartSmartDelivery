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
        static string s_connectionString;

        // Initialize the connection string from app.config
        public static void Initialize(string ConnectionStringName)
        {
            // Retrieve the connection string from app.config
            s_connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString;

            if (string.IsNullOrEmpty(s_connectionString))
            {
                throw new InvalidOperationException("Connection string not found in configuration file.");
            }
        }

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(s_connectionString))
                {
                    throw new InvalidOperationException("Connection string not initialized. Call Initialize() first.");
                }
                return s_connectionString;
            }
        }
    }

}
