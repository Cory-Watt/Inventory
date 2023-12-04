using System;
using Inventory.Models;
using Inventory.ViewModels;
using Microsoft.Data.SqlClient;

// Namespace for organizing services related to data access
namespace Inventory.Services
{
    // UserDAO class for direct operations with the database concerning user data
    public class UserDAO
    {
        // Connection string for accessing the SQL database
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=InventoryDB;Integrated Security=True; Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        // Method to register a new user in the database
        public bool RegisterUser(UserModel user)
        {
            // Flag to indicate success or failure of the operation
            bool success = false;
            // SQL statement for inserting a new user record into the User table
            string sqlStatement = "INSERT INTO dbo.[User] (FirstName, LastName, Email, UserName, Password) VALUES (@FirstName, @LastName, @Email, @UserName, @Password)";

            // Using block ensures that the SQL connection is closed and disposed properly
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Command setup with the SQL statement and connection
                SqlCommand command = new SqlCommand(sqlStatement, connection);
                // Add parameters to prevent SQL injection attacks
                command.Parameters.Add("@FirstName", System.Data.SqlDbType.NVarChar, 100).Value = user.FirstName ?? string.Empty;
                command.Parameters.Add("@LastName", System.Data.SqlDbType.NVarChar, 100).Value = user.LastName ?? string.Empty;
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 200).Value = user.Email ?? string.Empty;
                command.Parameters.Add("@UserName", System.Data.SqlDbType.NVarChar, 100).Value = user.UserName ?? string.Empty;
                command.Parameters.Add("@Password", System.Data.SqlDbType.NVarChar, 100).Value = user.Password ?? string.Empty;

                try
                {
                    // Open the database connection
                    connection.Open();
                    // Execute the command and store the number of affected rows
                    int result = command.ExecuteNonQuery();
                    // If one or more rows are affected, the operation was successful
                    success = result > 0;
                }
                catch (Exception ex)
                {
                    // If an exception occurs, log it to the console (or handle accordingly in production)
                    Console.WriteLine(ex.ToString());
                }

                // Return the success flag indicating if the operation was successful
                return success;
            }
        }

        // Method to check if a user can log in with the provided credentials
        public bool LoginUser(LoginViewModel user)
        {
            // Flag to indicate success or failure of the operation
            bool success = false;

            // SQL statement to select the user record where the username and password match
            string sqlStatement = "SELECT * FROM dbo.[User] WHERE UserName = @UserName AND Password = @Password";

            // Using block to ensure proper disposal of SQL connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Command setup with SQL statement and connection
                SqlCommand command = new SqlCommand(sqlStatement, connection);
                // Add parameters to prevent SQL injection attacks
                command.Parameters.Add("@UserName", System.Data.SqlDbType.NVarChar, 100).Value = user.UserName ?? string.Empty;
                command.Parameters.Add("@Password", System.Data.SqlDbType.NVarChar, 100).Value = user.Password ?? string.Empty;

                try
                {
                    // Open the database connection
                    connection.Open();
                    // Execute the command and use the reader to read the results
                    SqlDataReader reader = command.ExecuteReader();

                    // If there are rows, then the username and password are correct
                    if (reader.HasRows)
                    {
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    // If an exception occurs, log it to the console (or handle accordingly in production)
                    Console.WriteLine(e.Message);
                }

                // Return the success flag indicating if the login was successful
                return success;
            }
        }
    }
}