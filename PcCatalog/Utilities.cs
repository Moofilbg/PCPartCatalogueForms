﻿using System;
using System.Data;
using System.Windows.Forms;

using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace PcCatalog
{
    static class Utilities
    {
        public static double price;
        
        public static DataTable ClientProductsDataTable(string product)
        {
            string connectionString = "server=localhost;user=root;database=sys;port=3306;password=root";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            string productQuery = $"SELECT item, {product}_price FROM sys.{product} WHERE status = '1'";

            using (MySqlCommand command = new MySqlCommand(productQuery, connection))
            {
                command.CommandType = CommandType.Text;
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    using (DataTable ProductsDataTable = new DataTable())
                    {
                        adapter.Fill(ProductsDataTable);
                        connection.Close();
                        return ProductsDataTable;                                
                    }
                }
            }             
        }
        public static DataTable AdminProductsDataTable(string product)
        {
            string connectionString = "server=localhost;user=root;database=sys;port=3306;password=root";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            string productQuery = $"SELECT * FROM sys.{product}";

            using (MySqlCommand command = new MySqlCommand(productQuery, connection))
            {
                command.CommandType = CommandType.Text;
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    using (DataTable ProductsDataTable = new DataTable())
                    {
                        adapter.Fill(ProductsDataTable);
                        connection.Close();
                        return ProductsDataTable;
                    }
                }
            }
        }
        public static MySqlConnection ConnectionOpen()
        {
            string connectionString = "server=localhost;user=root;database=sys;port=3306;password=root";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }
        public static string DisplayPrice(double currentProductPrice,string command)
        {
            if (command == "add")
            {
                price += currentProductPrice;
                return Math.Abs(Math.Round(price, 2)).ToString();
            }
            else
            {
                price -= currentProductPrice;              
                return Math.Abs(Math.Round(price, 2)).ToString();
            }
        }

        public static int GetProductId(string database,string product,string price)
        {
            MySqlConnection connection = ConnectionOpen();

            string productIdQuery = $"SELECT product_id FROM sys.{database} WHERE item='{product}' AND {database}_price={price}";
            MySqlCommand command = new(productIdQuery, connection);
            int id = int.Parse(command.ExecuteScalar().ToString());
            connection.Close();
            return id;
        }

        public static void RemoveSelectedProducts(List<string> product, DataGridView grid)
        {
            for(int i = 0; i < grid.Rows.Count;i++)
            {
                for(int k = 0; k < product.Count;k++)
                {
                    if(product[k] == grid.Rows[i].Cells[1].Value.ToString())
                    {
                        grid.Rows.Remove(grid.Rows[i]);
                    }
                }
            }   
        }

        public static void CheckForAddedItems(List<string> product,DataGridView grid)
        {
            if (product.Count > 0)
            {
                RemoveSelectedProducts(product, grid);
            }
        }
        
        public static bool ClientChecker(string firstName, string lastName, string phoneNum, int customerIdCount)
        {
            string currentFirstName, currentLastName, currentPhoneNum;
            if (customerIdCount == 0)
            { return false; }

            MySqlConnection connection = ConnectionOpen();
            MySqlCommand command = new();

            string firstNameString = $"SELECT first_name FROM sys.customers WHERE customer_id = {customerIdCount}";
            command = new(firstNameString, connection);
            currentFirstName = command.ExecuteScalar().ToString();
            connection.Close();

            connection.Open();
            string lastNameString = $"SELECT last_name FROM sys.customers WHERE customer_id = {customerIdCount}";
            command = new(lastNameString, connection);
            currentLastName = command.ExecuteScalar().ToString();
            connection.Close();

            connection.Open();
            string phoneString = $"SELECT phone FROM sys.customers WHERE customer_id = {customerIdCount}";
            command = new(phoneString, connection);
            currentPhoneNum = command.ExecuteScalar().ToString();
            connection.Close();

            if (currentFirstName == firstName && currentLastName == lastName && currentPhoneNum == phoneNum)
            { return true; }
            else
            { return ClientChecker(firstName, lastName, phoneNum, customerIdCount - 1); }
        }

        public static void AddNewClient(string firstName,string lastName,string phoneNum)
        {
            MySqlConnection connection = ConnectionOpen();
            MySqlCommand command = new();        
            string customerInfo = "INSERT INTO sys.customers(first_name,last_name,phone) VALUES (@first,@last,@phone)";

            MySqlParameter firstNameParam = new();
            firstNameParam.ParameterName = "@first";
            firstNameParam.Value = firstName;

            MySqlParameter lastNameParam = new();
            lastNameParam.ParameterName = "@last";
            lastNameParam.Value = lastName;

            MySqlParameter phoneParameter = new();
            phoneParameter.ParameterName = "@phone";
            phoneParameter.Value = phoneNum;

            command = new(customerInfo, connection);
            command.Parameters.Add(firstNameParam);
            command.Parameters.Add(lastNameParam);
            command.Parameters.Add(phoneParameter);

            MySqlDataReader reader = command.ExecuteReader();
            connection.Close();
        }
  
        public static int GetCustomerCount()
        {
            MySqlConnection connection = ConnectionOpen();
            string customerIdQuery = $"SELECT COUNT(*) FROM sys.customers";
            MySqlCommand command = new(customerIdQuery, connection);
            int customerIdCount = int.Parse(command.ExecuteScalar().ToString());
            return customerIdCount;
        }

        public static int GetCustomerId(string firstName,string lastName,string phoneNum)
        {
            MySqlConnection connection = ConnectionOpen();
            string userIDString = $"SELECT customer_id FROM sys.customers WHERE first_name = '{firstName}' AND phone ='{phoneNum}' AND last_name ='{lastName}'";
            MySqlCommand userIdCommand = new(userIDString, connection);
            int userID = int.Parse(userIdCommand.ExecuteScalar().ToString()); // for report
            connection.Close();
            return userID;
        }

        public static int GetProductCount(string product)
        {
            MySqlConnection connection = ConnectionOpen();
            string productCountQuery = $"SELECT COUNT(*) FROM sys.{product} WHERE status = '1'";
            MySqlCommand command = new(productCountQuery,connection);
            int productCount = int.Parse(command.ExecuteScalar().ToString());
            return productCount;
        }
        public static List<string> ProductIdList(string product)
        {         
            MySqlConnection connection = ConnectionOpen();
            List<string> idList = new();

            string idQuery = $"SELECT product_id FROM sys.{product} WHERE status = '1'";
            MySqlCommand command = new(idQuery, connection);
            MySqlDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                idList.Add(reader[0].ToString());
            }
            connection.Close();
            return idList;
        }

        public static void PurchaseReport(int userID,int productID,string product,DateTime time,double productPrice)
        {
            MySqlConnection connection = ConnectionOpen();
            string report = "INSERT INTO sys.report(customer_id,product_id,product,date,price,removed) " +
                    "VALUES (@customer,@productid,@product,@date,@price,@removed)";

            MySqlParameter customerParam = new();
            customerParam.ParameterName = "@customer";
            customerParam.Value = userID;

            MySqlParameter productIdParam = new();
            productIdParam.ParameterName = "@productid";
            productIdParam.Value = productID;

            MySqlParameter productParam = new();
            productParam.ParameterName = "@product";
            productParam.Value = product;

            MySqlParameter timeParam = new();
            timeParam.ParameterName = "@date";
            timeParam.Value = time;

            MySqlParameter priceParam = new();
            priceParam.ParameterName = "@price";
            priceParam.Value = productPrice;

            MySqlParameter removedParam = new();// if the product is removed from the entire database
            removedParam.ParameterName = "@removed";
            removedParam.Value = "0";

            MySqlCommand reportInsertion = new(report, connection);
            reportInsertion.Parameters.Add(customerParam);
            reportInsertion.Parameters.Add(productIdParam);
            reportInsertion.Parameters.Add(timeParam);
            reportInsertion.Parameters.Add(productParam);
            reportInsertion.Parameters.Add(priceParam);
            reportInsertion.Parameters.Add(removedParam);

            MySqlDataReader reader = reportInsertion.ExecuteReader();
            connection.Close();
        }

        public static void AddProductToDataBase(string model,string price,string status,string query)
        {
            MySqlConnection connection = ConnectionOpen();

            MySqlParameter itemParameter = new();
            itemParameter.ParameterName = "@item";
            itemParameter.Value = model;

            MySqlParameter priceParameter = new();
            priceParameter.ParameterName = "@price";
            priceParameter.Value = price;

            MySqlParameter statusParameter = new();
            statusParameter.ParameterName = "@status";
            statusParameter.Value = status;

            MySqlCommand command = new(query, connection);
            command.Parameters.Add(itemParameter);
            command.Parameters.Add(priceParameter);
            command.Parameters.Add(statusParameter);

            MySqlDataReader reader = command.ExecuteReader();
            connection.Close();

            //whipe text\\
           // WhipeText();

            MessageBox.Show("New product saved!");
        }
        public static void CorrectionToDataBase(string model,string price,string status,string query)
        {
            MySqlConnection connection = ConnectionOpen();

            MySqlParameter itemParameter = new();
            itemParameter.ParameterName = "@item";
            itemParameter.Value = model;

            MySqlParameter priceParameter = new();
            priceParameter.ParameterName = "@price";
            priceParameter.Value = price;

            MySqlParameter statusParameter = new();
            statusParameter.ParameterName = "@status";
            statusParameter.Value = status;

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.Add(itemParameter);
            command.Parameters.Add(priceParameter);
            command.Parameters.Add(statusParameter);

            MySqlDataReader reader = command.ExecuteReader();
            connection.Close();

            MessageBox.Show("Correction made successfully!");
        }
        public static void RemoveProductFromDataBase(string model,string query)
        {
            MySqlConnection connection = ConnectionOpen();

            MySqlParameter itemParameter = new();
            itemParameter.ParameterName = "@item";
            itemParameter.Value = model;

            MySqlCommand command = new (query, connection);
            command.Parameters.Add(itemParameter);           

            MySqlDataReader reader = command.ExecuteReader();
            MessageBox.Show("Item successfully removed!");

            connection.Close();

            CheckForBoughtProducts(model);
        }
        private static void CheckForBoughtProducts(string model)
        {
            MySqlConnection connection = ConnectionOpen();

            string countQuery = $"SELECT COUNT(*) FROM sys.report WHERE product ='{model}'";
            MySqlCommand command = new(countQuery, connection);
            int count = int.Parse(command.ExecuteScalar().ToString());

            connection.Close();

            if (count > 0)
            {
                connection.Open();

                string removedUpdate = $"UPDATE sys.report SET removed = '1' WHERE product ='{model}'";
                MySqlCommand recordCommand = new(removedUpdate, connection);
                MySqlDataReader reader = recordCommand.ExecuteReader();

                connection.Close();
            }
        }

        public static List<string> RepeatedItemsInReport()
        {
            MySqlConnection connnection = ConnectionOpen();

            string duplicateItemsInQuery = $"SELECT product FROM sys.report GROUP BY product HAVING COUNT(product)>1";
            MySqlCommand command = new(duplicateItemsInQuery, connnection);
            MySqlDataReader reader = command.ExecuteReader();

            List<string> repeatedItemsList = new();

            while (reader.Read())
            {
                repeatedItemsList.Add(reader[0].ToString());
            }
            connnection.Close();

            return repeatedItemsList;
        }

        public static double TotalIncomePerRepeatedProduct(List<string> repeatedItemList,int productIndex)
        {
            MySqlConnection connection = new();          

            string itemPriceQuery= $"SELECT price FROM sys.report WHERE product ='{repeatedItemList[productIndex]}'"; // gets the price of the duplicate item
            MySqlCommand command = new(itemPriceQuery, connection);
            double itemPrice = double.Parse(command.ExecuteScalar().ToString());
            connection.Close();

            return itemPrice;
        }

        public static int TotalOrdersPerRepeatedProduct(List<string> repeatedItemList, int productIndex)
        {
            MySqlConnection connection = new();

            string ordersQuery = $"SELECT COUNT(*) FROM sys.report WHERE product = '{repeatedItemList[productIndex]}'";
            MySqlCommand command = new(ordersQuery,connection);
            int orders = int.Parse(command.ExecuteScalar().ToString());
            connection.Close();

            return orders;
        }

        
        
        
    }
}
