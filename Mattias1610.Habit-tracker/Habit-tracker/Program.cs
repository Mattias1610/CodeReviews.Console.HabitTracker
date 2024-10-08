﻿using System;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace HabitTracker
{
    public class Program
    {
        private static string connectionString = @"Data source=habit-tracker.db";
        private static string habitName;
        private static string measurementUnit;

        public static void Main(string[] args)
        {
            try
            {
                habitName = GetHabitName("\t What habit do you want to track?");
                measurementUnit = GetMeasurement();

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var tableCmd = connection.CreateCommand();
                    tableCmd.CommandText =
                        @$"CREATE TABLE IF NOT EXISTS {habitName}(
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Date TEXT,
                            {measurementUnit} INTEGER
                        )";
                    tableCmd.ExecuteNonQuery();

                    tableCmd.CommandText = $"SELECT COUNT(*) FROM {habitName}";
                    int recordCount = Convert.ToInt32(tableCmd.ExecuteScalar());

                    if (recordCount == 0)
                    {
                        InsertRandomRecords(connection);
                    }

                    connection.Close();
                }

                GetUserInput();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static string GetHabitName(string messageHabit)
        {
            try
            {
                Console.Clear();
                Console.WriteLine(messageHabit);
                return Console.ReadLine().Replace(" ", "_");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }

        private static string GetMeasurement()
        {
            try
            {
                Console.WriteLine("What's the unit of measurement?");
                return Console.ReadLine().Replace(" ", "_");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }

        private static void InsertRandomRecords(SqliteConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var tableCmd = connection.CreateCommand();
                    tableCmd.Transaction = transaction;
                    Random random = new Random();
                    for (int i = 0; i < 100; i++)
                    {
                        string date = GenerateRandomDate(random);
                        int quantity = random.Next(1, 101);

                        tableCmd.CommandText = $"INSERT INTO {habitName}(Date, {measurementUnit}) VALUES ('{date}', {quantity})";
                        tableCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        private static string GenerateRandomDate(Random random)
        {
            try
            {
                int day = random.Next(1, 29);
                int month = random.Next(1, 13);
                DateTime date = new DateTime(2024, month, day);
                return date.ToString("dd-MM-yy");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while generating a random date: {ex.Message}");
                return string.Empty;
            }
        }

        private static void GetUserInput()
        {
            Console.Clear();
            bool isRunning = true;

            while (isRunning)
            {
                try
                {
                    Console.WriteLine("\t MAIN MENU");
                    Console.WriteLine("0 - EXIT");
                    Console.WriteLine("1 - View Records");
                    Console.WriteLine("2 - Add Record");
                    Console.WriteLine("3 - Update Record");
                    Console.WriteLine("4 - Delete Record");
                    Console.WriteLine("5 - See sum");

                    string command = Console.ReadLine();

                    switch (command)
                    {
                        case "0":
                            isRunning = false;
                            Environment.Exit(0);
                            break;
                        case "1":
                            ViewRecords();
                            break;
                        case "2":
                            Insert();
                            break;
                        case "3":
                            Update();
                            break;
                        case "4":
                            Delete();
                            break;
                        case "5":
                            SeeStats();
                            break;
                        default:
                            Console.WriteLine("Invalid number");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        private static void Insert()
        {
            try
            {
                string date = GetDateInput();
                int quantity = GetNumberInput($"\n Insert the {measurementUnit}");

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var tableCmd = connection.CreateCommand();
                    tableCmd.CommandText = $"INSERT INTO {habitName}(Date, {measurementUnit}) VALUES ('{date}', '{quantity}')";
                    tableCmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static string GetDateInput()
        {
            try
            {
                Console.WriteLine("INSERT THE DATE IN THIS FORMAT DD/MM/YY \t 0 - To go back to the main menu");
                string dateInput = Console.ReadLine();

                if (dateInput == "0") GetUserInput();

                DateTime parsedDate = DateTime.ParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"));
                return parsedDate.ToString("dd-MM-yy");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid date format: {ex.Message}");
                return GetDateInput();
            }
        }

        private static int GetNumberInput(string message)
        {
            try
            {
                Console.WriteLine(message);
                string numInput = Console.ReadLine();
                return Convert.ToInt32(numInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid input: {ex.Message}");
                return GetNumberInput(message);
            }
        }

        private static void ViewRecords()
        {
            Console.Clear();
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var tableCmd = connection.CreateCommand();
                    tableCmd.CommandText = $"SELECT * FROM {habitName}";

                    List<DrinkingWater> tableData = new();
                    SqliteDataReader reader = tableCmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            tableData.Add(new DrinkingWater
                            {
                                Id = reader.GetInt32(0),
                                Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                                Quantity = reader.GetInt32(2)
                            });
                        }
                    }

                    connection.Close();

                    Console.WriteLine("------------------------------------------\n");
                    foreach (var dw in tableData)
                    {
                        Console.WriteLine($"{dw.Id} - {dw.Date:dd-MMM-yyyy} - {measurementUnit}: {dw.Quantity}");
                    }
                    Console.WriteLine("------------------------------------------\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public class DrinkingWater
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public int Quantity { get; set; }
        }

        private static void Delete()
        {
            Console.Clear();
            ViewRecords();

            try
            {
                var recordId = GetNumberInput("\n Enter the ID of the record you want to delete");

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var tableCmd = connection.CreateCommand();
                    tableCmd.CommandText = $"DELETE FROM {habitName} WHERE Id='{recordId}'";
                    int rowCount = tableCmd.ExecuteNonQuery();

                    if (rowCount == 0)
                    {
                        Console.WriteLine("Record doesn't exist");
                        Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void Update()
        {
            ViewRecords();

            try
            {
                var recordId = GetNumberInput("\n\nPlease type Id of the record you would like to update. Type 0 to return to the main menu.\n\n");

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var checkCmd = connection.CreateCommand();
                    checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM {habitName} WHERE Id = {recordId})";
                    int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (checkQuery == 0)
                    {
                        Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist.\n\n");
                        connection.Close();
                        Update();
                    }

                    string date = GetDateInput();
                    int quantity = GetNumberInput($"\n\nPlease insert the quantity (no decimals allowed)\n\n");

                    var tableCmd = connection.CreateCommand();
                    tableCmd.CommandText = $"UPDATE {habitName} SET Date = '{date}', {measurementUnit} = {quantity} WHERE Id = {recordId}";
                    tableCmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void SeeStats()
        {
            int sum = 0;

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var tableCmd = connection.CreateCommand();
                    tableCmd.CommandText = $"SELECT * FROM {habitName}";

                    List<DrinkingWater> tableData = new();
                    SqliteDataReader reader = tableCmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            tableData.Add(new DrinkingWater
                            {
                                Id = reader.GetInt32(0),
                                Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                                Quantity = reader.GetInt32(2)
                            });
                        }
                    }

                    connection.Close();

                    foreach (var dw in tableData)
                    {
                        sum += dw.Quantity;
                    }

                    Console.WriteLine($"The sum of {measurementUnit} is {sum}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
