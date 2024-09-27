using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Configuration;
using SmartStartDeliveryForm.Classes;
using SmartStartDeliveryForm.DTOs;
using SmartStartDeliveryForm.Enums;

namespace SmartStartDeliveryForm.DAOs
{
/*
DAO:
Encapsulates the data access operations (like querying, inserting, updating, and deleting data).
Provides an interface to interact with the data source (such as a database).
Decouples the data access code from the rest of the application
*/
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
                    string Query = @"SELECT * FROM Drivers;";

                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        using (SqlDataAdapter Adapter = new SqlDataAdapter(Command))
                        {
                            Adapter.Fill(Dt);

                            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
                            PrimaryKeyColumns[0] = Dt.Columns["DriverID"];
                            Dt.PrimaryKey = PrimaryKeyColumns;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("An error occurred while accessing the database: " + ex.Message);
                    return null;
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
                    string Query = "SELECT TOP 1 * FROM Drivers WHERE EmployeeNo = @EmployeeNo";

                    using (var Command = new SqlCommand(Query, Connection))
                    {
                        Command.CommandType = CommandType.Text; // Since it's a direct SQL query
                        Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = EmployeeNo });

                        Connection.Open();

                        object FoundRow = Command.ExecuteScalar();
                        int Result = (FoundRow != null) ? (int)FoundRow : 0; 

                        return Result; // 1 if the EmployeeNo exists, 0 if not
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("An error occurred while accessing the database: " + ex.Message);
                    return 1; // Assume it's not unique on error
                }
            }
        }

        public static void DeleteDriver(int DriverID)
        {
            using (SqlConnection Connection = new SqlConnection(_ConnectionString))
            {
                try
                {
                    string Query = "DELETE FROM Drivers WHERE DriverID = @DriverID";

                    using (SqlCommand command = new SqlCommand(Query, Connection))
                    {
                        command.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = DriverID });

                        Connection.Open();
                        int RowsAffected = command.ExecuteNonQuery();

                        if (RowsAffected > 0)
                        {
                            FormConsole.Instance.Log($"{RowsAffected} row(s) deleted.");
                        }
                        else
                        {
                            FormConsole.Instance.Log("No rows were deleted. Employee may not exist.");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("An error occurred while accessing the database: " + ex.Message);
                }
            }
        }

        public static int InsertDriver(DriversDTO driver)
        {
            using (SqlConnection Connection = new SqlConnection(_ConnectionString))
            {
                try
                {
                    string Query = @"
                INSERT INTO Drivers (Name, Surname, EmployeeNo, LicenseType, Availability) 
                VALUES (@Name, @Surname, @EmployeeNo, @LicenseType, @Availability);
                SELECT CAST(SCOPE_IDENTITY() AS int);"; // Get the new DriverID

                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        // Add parameters
                        Command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                        Command.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                        Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                        Command.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                        Command.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });

                        Connection.Open();
                        int newDriverId = (int)Command.ExecuteScalar(); 

                        FormConsole.Instance.Log("Driver added successfully with ID: " + newDriverId);
                        return newDriverId; 
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("An error occurred while adding the driver: " + ex.Message);
                    return -1; //Indicates an error occurred
                }
            }
        }


        public static void UpdateDriver(DriversDTO driver)
        {
            using (SqlConnection Connection = new SqlConnection(_ConnectionString))
            {
                try
                {
                    string Query = "UPDATE Drivers SET Name = @Name, Surname = @Surname, EmployeeNo = @EmployeeNo, LicenseType = @LicenseType, Availability = @Availability WHERE DriverID = @DriverID;";
                    using (SqlCommand Command = new SqlCommand(Query, Connection))
                    {
                        // Add parameters to the command
                        Command.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = driver.DriverId });
                        Command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                        Command.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                        Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                        Command.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                        Command.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });

                        Connection.Open();
                        int RowsAffected = Command.ExecuteNonQuery();
                        FormConsole.Instance.Log($"{RowsAffected} row(s) updated.");
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
