using System;
using Microsoft.Data.SqlClient;
using Inventory.Models;
using System.Linq.Expressions;

namespace Inventory.Services
{
    public class InventoryDAO
    {
        // Connection string for accessing the database
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=InventoryDB;Integrated Security=True; Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        // Method to get all inventory items from the database
        public List<ItemModel> GetAllInventoryItems()
        {
            string sqlStatement = "SELECT * FROM dbo.[Item]";
            return FetchItems(sqlStatement, null);
        }

        // Method to search for inventory items based on search term or item ID
        public List<ItemModel> SearchForItem(string searchTerm, int? itemId, int pageNumber, int itemsPerPage)
        {
            string sqlStatement = @"
        SELECT * FROM dbo.[Item] WHERE 1=1";

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            // Adding search conditions to the SQL query
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sqlStatement += " AND ItemName LIKE @ItemName";
                parameters.Add("@ItemName", $"%{searchTerm}%");
            }

            if (itemId.HasValue)
            {
                sqlStatement += " AND ItemId = @ItemId";
                parameters.Add("@ItemId", itemId.Value);
            }

            // Add OFFSET and FETCH for pagination
            sqlStatement += " ORDER BY ItemId OFFSET @Offset ROWS FETCH NEXT @RowsPerPage ROWS ONLY";
            parameters.Add("@Offset", (pageNumber - 1) * itemsPerPage);
            parameters.Add("@RowsPerPage", itemsPerPage);

            return FetchItems(sqlStatement, parameters);
        }


        // Method to get items for a specific page
        public List<ItemModel> GetItems(int pageNumber, int itemsPerPage)
        {
            var items = new List<ItemModel>();
            string sqlStatement = @"
                SELECT * FROM dbo.Item
                ORDER BY ItemId
                OFFSET @Offset ROWS
                FETCH NEXT @RowsPerPage ROWS ONLY";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);
                command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * itemsPerPage);
                command.Parameters.AddWithValue("@RowsPerPage", itemsPerPage);

                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new ItemModel
                            {
                                ItemId = (int)reader["ItemId"],
                                ItemName = (string)reader["ItemName"],
                                ItemDescription = (string)reader["ItemDescription"],
                                ItemPrice = (decimal)reader["ItemPrice"],
                                ItemQuantity = (int)reader["ItemQuantity"],
                                ItemVendorName = (string)reader["ItemVendorName"],
                            };
                            items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    Console.WriteLine(ex.Message);
                }
            }

            return items;
        }

        // Method to add a new item to the inventory
        public bool AddNewItem(ItemModel item)
        {
            string sqlStatement = "INSERT INTO dbo.[Item] (ItemName, ItemDescription, ItemPrice, ItemQuantity, ItemVendorId, ItemVendorName, " +
                "ItemVendorContactDetails, ItemVendorAssociatedProducts) VALUES (@ItemName, @ItemDescription, @ItemPrice, @ItemQuantity, @ItemVendorId, " +
                "@ItemVendorName, @ItemVendorContactDetails, @ItemVendorAssociatedProducts)";

            // Database operation within using block for resource management
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);
                // Adding parameters to the SQL command
                command.Parameters.AddWithValue("@ItemName", item.ItemName);
                command.Parameters.AddWithValue("@ItemDescription", item.ItemDescription);
                command.Parameters.AddWithValue("@ItemPrice", item.ItemPrice);
                command.Parameters.AddWithValue("@ItemQuantity", item.ItemQuantity);
                command.Parameters.AddWithValue("@ItemVendorId", item.ItemVendorId);
                command.Parameters.AddWithValue("@ItemVendorName", item.ItemVendorName);
                command.Parameters.AddWithValue("@ItemVendorContactDetails", item.ItemVendorContactDetails);
                command.Parameters.AddWithValue("@ItemVendorAssociatedProducts", item.ItemVendorAssociatedProducts);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0; // Return true if the item was successfully added
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        }

        public ItemModel GetItemById(int itemId)
        {
            ItemModel item = null;
            string sqlStatement = "SELECT * FROM dbo.[Item] WHERE ItemId = @ItemId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);
                command.Parameters.AddWithValue("@ItemId", itemId);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item = new ItemModel
                            {
                                ItemId = (int)reader["ItemId"],
                                ItemName = (string)reader["ItemName"],
                                ItemDescription = (string)reader["ItemDescription"],
                                ItemPrice = (decimal)reader["ItemPrice"],
                                ItemQuantity = (int)reader["ItemQuantity"],
                                ItemVendorId = reader.IsDBNull(reader.GetOrdinal("ItemVendorId")) ? 0 : (int)reader["ItemVendorId"],
                                ItemVendorName = reader.IsDBNull(reader.GetOrdinal("ItemVendorName")) ? null : (string)reader["ItemVendorName"],
                                ItemVendorContactDetails = reader.IsDBNull(reader.GetOrdinal("ItemVendorContactDetails")) ? null : (string)reader["ItemVendorContactDetails"],
                                ItemVendorAssociatedProducts = reader.IsDBNull(reader.GetOrdinal("ItemVendorAssociatedProducts")) ? null : (string)reader["ItemVendorAssociatedProducts"]
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return item;
        }

        public List<ItemModel> GetItemsForReport()
        {
            List<ItemModel> items = new List<ItemModel>();
            string sqlStatement = "SELECT * FROM dbo.[Item]";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read()) // Use while instead of if
                        {
                            ItemModel item = new ItemModel
                            {
                                ItemId = (int)reader["ItemId"],
                                ItemName = (string)reader["ItemName"],
                                ItemDescription = (string)reader["ItemDescription"],
                                ItemPrice = (decimal)reader["ItemPrice"],
                                ItemQuantity = (int)reader["ItemQuantity"],
                                ItemVendorId = reader.IsDBNull(reader.GetOrdinal("ItemVendorId")) ? 0 : (int)reader["ItemVendorId"],
                                ItemVendorName = reader.IsDBNull(reader.GetOrdinal("ItemVendorName")) ? null : (string)reader["ItemVendorName"],
                                ItemVendorContactDetails = reader.IsDBNull(reader.GetOrdinal("ItemVendorContactDetails")) ? null : (string)reader["ItemVendorContactDetails"],
                                ItemVendorAssociatedProducts = reader.IsDBNull(reader.GetOrdinal("ItemVendorAssociatedProducts")) ? null : (string)reader["ItemVendorAssociatedProducts"]
                            };
                            items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return items;
        }

        // Method to update an existing item in the inventory
        public bool UpdateItem(ItemModel item)
        {
            string sqlStatement = "UPDATE dbo.[Item] SET ItemName = @ItemName, ItemDescription = @ItemDescription, " +
                      "ItemPrice = @ItemPrice, ItemQuantity = @ItemQuantity, ItemVendorId = @ItemVendorId, " +
                      "ItemVendorName = @ItemVendorName, ItemVendorContactDetails = @ItemVendorContactDetails, " +
                      "ItemVendorAssociatedProducts = @ItemVendorAssociatedProducts WHERE ItemId = @ItemId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);
                // Adding parameters for the update operation
                command.Parameters.AddWithValue("@ItemId", item.ItemId);
                command.Parameters.AddWithValue("@ItemName", item.ItemName);
                command.Parameters.AddWithValue("@ItemDescription", item.ItemDescription);
                command.Parameters.AddWithValue("@ItemPrice", item.ItemPrice);
                command.Parameters.AddWithValue("@ItemQuantity", item.ItemQuantity);
                command.Parameters.AddWithValue("@ItemVendorId", item.ItemVendorId);
                command.Parameters.AddWithValue("@ItemVendorName", item.ItemVendorName);
                command.Parameters.AddWithValue("@ItemVendorContactDetails", item.ItemVendorContactDetails);
                command.Parameters.AddWithValue("@ItemVendorAssociatedProducts", item.ItemVendorAssociatedProducts);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0; // Return true if the update was successful
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        }

        public bool DeleteItem(int itemId)
        {
            string sqlStatement = "DELETE FROM dbo.[Item] WHERE ItemId = @ItemId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);
                command.Parameters.AddWithValue("@ItemId", itemId);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0; // Return true if the item was successfully deleted
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        }

        public int CountItems(string searchTerm, int? itemId)
        {
            int count = 0;
            string sqlStatement = "SELECT COUNT(*) FROM dbo.Item WHERE 1=1";
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            // Adding search conditions to the SQL query
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sqlStatement += " AND ItemName LIKE @ItemName";
                parameters.Add("@ItemName", $"%{searchTerm}%");
            }

            if (itemId.HasValue)
            {
                sqlStatement += " AND ItemId = @ItemId";
                parameters.Add("@ItemId", itemId.Value);
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);

                // Add parameters to the command
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }

                try
                {
                    connection.Open();
                    count = (int)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    // Handle exception
                    Console.WriteLine(ex.Message);
                }
            }

            return count;
        }

        // Generic method to fetch items from the database based on a SQL query
        public List<ItemModel> FetchItems(string sqlStatement, Dictionary<string, object> parameters)
        {
            List<ItemModel> items = new List<ItemModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);

                // Add parameters to the command if they exist
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(ItemsFromReader(reader)); // Add items to the list based on the database records
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return items;
        }

        // Helper method to create an ItemModel object from a SqlDataReader
        private ItemModel ItemsFromReader(SqlDataReader reader)
        {
            // Creating and returning an ItemModel populated with data from the reader
            return new ItemModel
            {
                ItemId = (int)reader["ItemId"],
                ItemName = (string)reader["ItemName"],
                ItemDescription = (string)reader["ItemDescription"],
                ItemPrice = (decimal)reader["ItemPrice"],
                ItemQuantity = (int)reader["ItemQuantity"],
                ItemVendorName = (string)reader["ItemVendorName"],
            };
        }
    }
}