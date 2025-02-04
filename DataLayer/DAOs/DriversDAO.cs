﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.DataLayer.DAOs
{
    /*
    DAO:
    Encapsulates the data access operations (like querying, inserting, updating, and deleting data).
    Provides an interface to interact with the data source (such as a database).
    Decouples the data access code from the rest of the application
    */
    public class DriversDAO(IConfiguration configuration, string connectionString = null)
    {
        private readonly string _connectionString = connectionString ?? configuration["ConnectionStrings:StartSmartDB"]
                       ?? throw new InvalidOperationException("Connection string not found.");

        public DataTable? GetAllDrivers()
        {
            DataTable Dt = new();

            using (SqlConnection Connection = new(_connectionString))
            {
                try
                {
                    string Query = @"SELECT * FROM Drivers;";

                    using (SqlCommand Command = new(Query, Connection))
                    {
                        using (SqlDataAdapter Adapter = new(Command))
                        {
                            Adapter.Fill(Dt);

                            if (Dt.Columns.Contains("DriverID"))
                            {
                                DataColumn[] PrimaryKeyColumns = [Dt.Columns["DriverID"]!];
                                Dt.PrimaryKey = PrimaryKeyColumns;
                            }
                            else
                            {
                                throw new InvalidOperationException("The DataTable must contain the 'DriverID' column.");
                            }


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

        public int GetEmployeeNoCount(string EmployeeNo)
        {
            using (SqlConnection Connection = new(_connectionString))
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

        public void DeleteDriver(int DriverID, SqlTransaction transaction = null)
        {
            using (SqlConnection Connection = new(_connectionString))
            {
                try
                {
                    string Query = "DELETE FROM Drivers WHERE DriverID = @DriverID";

                    using (SqlCommand Command = new(Query, Connection))
                    {
                        Command.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = DriverID });

                        if (transaction != null)
                        {
                            Command.Transaction = transaction;
                        }

                        Connection.Open();
                        int RowsAffected = Command.ExecuteNonQuery();

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

        public int InsertDriver(DriversDTO driver, SqlTransaction transaction = null)
        {
            using (SqlConnection Connection = new(_connectionString))
            {
                try
                {
                    string Query = @"
          INSERT INTO Drivers (Name, Surname, EmployeeNo, LicenseType, Availability) 
          VALUES (@Name, @Surname, @EmployeeNo, @LicenseType, @Availability);
          SELECT CAST(SCOPE_IDENTITY() AS int);"; // Get the new DriverID

                    using (SqlCommand Command = new(Query, Connection))
                    {
                        // Add parameters
                        Command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                        Command.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                        Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                        Command.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                        Command.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });

                        if (transaction != null)
                        {
                            Command.Transaction = transaction;
                        }

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

        public void UpdateDriver(DriversDTO driver, SqlTransaction transaction = null)
        {
            using (SqlConnection Connection = new(_connectionString))
            {
                try
                {
                    string Query = "UPDATE Drivers SET Name = @Name, Surname = @Surname, EmployeeNo = @EmployeeNo, LicenseType = @LicenseType, Availability = @Availability WHERE DriverID = @DriverID;";
                    using (SqlCommand Command = new(Query, Connection))
                    {
                        // Add parameters to the command
                        Command.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = driver.DriverID });
                        Command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                        Command.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                        Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                        Command.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                        Command.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });

                        if (transaction != null)
                        {
                            Command.Transaction = transaction;
                        }

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

        public DataTable? GetDriversAtPage(int Page)
        {

            int Offset = (Page - 1) * GlobalConstants.s_recordLimit;
            DataTable Dt = new();

            using (SqlConnection Connection = new(_connectionString))
            {
                try
                {
                    string Query = @"
                    SELECT * FROM Drivers
                    ORDER BY DriverID
                    OFFSET @Offset ROWS 
                    FETCH NEXT @Pagelimit ROWS ONLY;";

                    using (SqlCommand Command = new(Query, Connection))
                    {
                        Command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = Offset });
                        Command.Parameters.Add(new SqlParameter("@Pagelimit", SqlDbType.Int) { Value = GlobalConstants.s_recordLimit });

                        using (SqlDataAdapter Adapter = new(Command))
                        {
                            Adapter.Fill(Dt);

                            DataColumn[] PrimaryKeyColumns = [Dt.Columns["DriverID"]!];
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

        internal int GetRecordCount()
        {
            int recordsCount = 0;

            string query = "SELECT COUNT(DriverID) FROM Drivers";

            using (SqlConnection connection = new(_connectionString)) // Ensure s_connectionString is defined
            {
                try
                {
                    using (SqlCommand command = new(query, connection))
                    {
                        connection.Open();
                        recordsCount = (int)command.ExecuteScalar(); // Execute the query and get the total count of records
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("Error counting records: " + ex.Message);
                    return recordsCount; // Return 0 pages if an error occurs
                }
            }

            return recordsCount;
        }

        public DataTable GetDriverByID(int driverID)
        {
            DataTable driverDataTable = new();
            string query = "SELECT DriverID, Name, Surname, EmployeeNo, LicenseType, Availability FROM Drivers WHERE DriverID = @DriverID";

            using (SqlConnection connection = new(_connectionString))
            {
                try
                {
                    using (SqlDataAdapter adapter = new(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@DriverID", driverID);
                        connection.Open();
                        adapter.Fill(driverDataTable);
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("Error retrieving driver by ID: " + ex.Message);
                }
            }

            return driverDataTable;
        }
    }
}
