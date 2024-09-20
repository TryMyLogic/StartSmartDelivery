using SmartStartDeliveryForm.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SmartStartDeliveryForm.DAOs
{
    using SmartStartDeliveryForm.DTOs;
    using SmartStartDeliveryForm.Enums;
    /*
*a DAO:
Encapsulates the data access operations (like querying, inserting, updating, and deleting data).
Provides an interface to interact with the data source (such as a database).
Decouples the data access code from the rest of the application
*/
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;

    internal static class DriversDAO
    {
        private static string _ConnectionString = DatabaseConfig.ConnectionString;

        public static DataTable GetAllDrivers()
        {
            DataTable Dt = new DataTable();

            using (SqlConnection Connection = new SqlConnection(_ConnectionString))
            {
                try
                {
                    string Query = @"
                SELECT Name, Surname, EmployeeNo,
                CASE LicenseType
                    WHEN 1 THEN 'Code8'
                    WHEN 2 THEN 'Code10'
                    WHEN 3 THEN 'Code14'
                    ELSE 'Unknown'
                END AS LicenseType,
                Availability
                FROM Drivers;";

                    using (SqlCommand command = new SqlCommand(Query, Connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(Dt);

                            // Set EmployeeNo as the primary key
                            DataColumn[] primaryKeyColumns = new DataColumn[1];
                            primaryKeyColumns[0] = Dt.Columns["EmployeeNo"];
                            Dt.PrimaryKey = primaryKeyColumns;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("An error occurred while accessing the database: " + ex.Message);
                }
            }

            return Dt;
        }

        public static int GetEmployeeNoCount(string EmployeeNo)
        {
            using (SqlConnection Connection = new SqlConnection(_ConnectionString))
            {
                try
                {
                    string Query = "proc_CheckEmployeeNoUnique";
                    using (var Command = new SqlCommand(Query, Connection))
                    {
                        Command.CommandType = CommandType.StoredProcedure;
                        Command.Parameters.AddWithValue("@EmployeeNo", EmployeeNo);
                        Connection.Open();
                        int Result = (int)Command.ExecuteScalar();
                        return Result;
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("An error occurred while accessing the database: " + ex.Message);
                    return 1; //Assume its not unique on error
                }
            }
        }

        public static void DeleteDriver(string EmployeeNo)
        {
            using (SqlConnection Connection = new SqlConnection(_ConnectionString))
            {
                try
                {
                    string Query = "DELETE FROM Drivers WHERE EmployeeNo = @EmployeeNo";

                    using (SqlCommand command = new SqlCommand(Query, Connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeNo", EmployeeNo);

                        Connection.Open();
                        int RowsAffected = command.ExecuteNonQuery();

                        if (RowsAffected > 0)
                        {
                            FormConsole.Instance.Log($"{RowsAffected} row(s) deleted.");
                        }
                        else
                        {
                            FormConsole.Instance.Log("No rows were deleted. Employee No may not exist.");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("An error occurred while accessing the database: " + ex.Message);
                }
            }
        }

        public static void InsertDriver(DriversDTO Driver)
        {
            using (SqlConnection Connection = new SqlConnection(_ConnectionString))
            {
                try
                {
                    Connection.Open();
                    string Query = "INSERT INTO Drivers (Name, Surname, EmployeeNo, LicenseType) VALUES (@Name, @Surname, @EmployeeNo, @LicenseType);";

                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        // Parameters prevent SQL injections and handle data properly
                        Command.Parameters.AddWithValue("@Name", Driver.Name);
                        Command.Parameters.AddWithValue("@Surname", Driver.Surname);
                        Command.Parameters.AddWithValue("@EmployeeNo", Driver.EmployeeNo);
                        Command.Parameters.AddWithValue("@LicenseType", (int)Driver.LicenseType); // Ensure LicenseType is a valid value (1, 2, or 3)

                        int RowsAffected = Command.ExecuteNonQuery();
                        FormConsole.Instance.Log($"{RowsAffected} row(s) inserted.");
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("An error occurred while accessing the database: " + ex.Message);
                }
            }
        }

        public static void UpdateDriver(DriversDTO driver)
        {
            using (SqlConnection Connection = new SqlConnection(_ConnectionString))
            {
                try
                {
                    string Query = "UPDATE Drivers SET Name = @Name, Surname = @Surname, LicenseType = @LicenseType WHERE EmployeeNo = @EmployeeNo;";
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.AddWithValue("@Name", driver.Name);
                        Command.Parameters.AddWithValue("@Surname", driver.Surname);
                        Command.Parameters.AddWithValue("@EmployeeNo", driver.EmployeeNo);
                        Command.Parameters.AddWithValue("@LicenseType", (int)driver.LicenseType);

                        Connection.Open();
                        int rowsAffected = Command.ExecuteNonQuery();
                        FormConsole.Instance.Log($"{rowsAffected} row(s) updated.");
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("An error occurred while accessing the database: " + ex.Message);
                }
            }
        }

    }
}
