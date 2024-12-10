using System;
using ClosedXML.Excel;
using System.Data.SqlClient;

class CashControlSystem
{
    static void Main(string[] args)
    {
        string connectionString = "Data Source=DESKTOP-AVA3DPU\\SQLEXPRESS;Initial Catalog=CashControlSystemDB;Integrated Security=True;TrustServerCertificate=True;";

        Console.WriteLine("1. Execute Deposit Transaction");
        Console.WriteLine("2. Search Daily Transaction and Display");
        Console.WriteLine("3. Export Daily Transaction to Excel");
        Console.Write("Choose an option: ");
        int option = int.Parse(Console.ReadLine());

        switch (option)
        {
            case 1:
                ExecuteDepositTransaction(connectionString);
                break;
            case 2:
                SearchDailyTransaction(connectionString);
                break;
            case 3:
                ExportDailyTransactionToExcel(connectionString);
                break;
            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }

    static void ExecuteDepositTransaction(string connectionString)
    {
        Console.Write("Enter Teller ID: ");
        int tellerID = int.Parse(Console.ReadLine());

        Console.Write("Enter Customer ID: ");
        int customerID = int.Parse(Console.ReadLine());

        Console.Write("Enter Transaction Type (Deposit): ");
        string transactionType = Console.ReadLine();

        Console.Write("Enter Currency: ");
        string currency = Console.ReadLine();

        Console.Write("Enter Amount: ");
        decimal amount = decimal.Parse(Console.ReadLine());

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                string query = "EXEC AddTransaction @TellerID, @CustomerID, @TransactionType, @Currency, @Amount";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TellerID", tellerID);
                    command.Parameters.AddWithValue("@CustomerID", customerID);
                    command.Parameters.AddWithValue("@TransactionType", transactionType);
                    command.Parameters.AddWithValue("@Currency", currency);
                    command.Parameters.AddWithValue("@Amount", amount);

                    command.ExecuteNonQuery();
                    Console.WriteLine("Transaction successful!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    static void SearchDailyTransaction(string connectionString)
    {
        Console.Write("Enter Date (YYYY-MM-DD): ");
        string date = Console.ReadLine();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                string query = @"
                    SELECT 
                        t.TransactionID AS No, 
                        c.CustomerCode, 
                        t.TransactionType, 
                        t.Currency, 
                        t.Amount, 
                        t.TransactionDate, 
                        te.TellerName AS BankPIC
                    FROM Transactions t
                    JOIN Customer c ON t.CustomerID = c.CustomerID
                    JOIN Teller te ON t.TellerID = te.TellerID
                    WHERE CAST(t.TransactionDate AS DATE) = @TransactionDate";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionDate", date);

                    SqlDataReader reader = command.ExecuteReader();

                    Console.WriteLine("No\tCustomerCode\tTransactionType\tCurrency\tAmount\tTransactionDate\tBankPIC");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["No"]}\t{reader["CustomerCode"]}\t{reader["TransactionType"]}\t" +
                                          $"{reader["Currency"]}\t{reader["Amount"]}\t{reader["TransactionDate"]}\t{reader["BankPIC"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    static void ExportDailyTransactionToExcel(string connectionString)
    {
        Console.Write("Enter Date (YYYY-MM-DD): ");
        string date = Console.ReadLine();
        //string filePath = $"DailyTransactions_{date.Replace("-", "")}.xlsx";
        string filePath = $@"C:\Users\andara\source\repos\CashControlSystem\CashControlSystem\DailyTransactions_{date:yyyyMMdd}.xlsx";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                string query = @"
                    SELECT 
                        t.TransactionID AS No, 
                        c.CustomerCode, 
                        t.TransactionType, 
                        t.Currency, 
                        t.Amount, 
                        t.TransactionDate, 
                        te.TellerName AS BankPIC
                    FROM Transactions t
                    JOIN Customer c ON t.CustomerID = c.CustomerID
                    JOIN Teller te ON t.TellerID = te.TellerID
                    WHERE CAST(t.TransactionDate AS DATE) = @TransactionDate";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionDate", date);

                    SqlDataReader reader = command.ExecuteReader();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Transactions");
                        worksheet.Cell(1, 1).Value = "No";
                        worksheet.Cell(1, 2).Value = "Customer Code";
                        worksheet.Cell(1, 3).Value = "Transaction Type";
                        worksheet.Cell(1, 4).Value = "Currency";
                        worksheet.Cell(1, 5).Value = "Amount";
                        worksheet.Cell(1, 6).Value = "Transaction Date";
                        worksheet.Cell(1, 7).Value = "Bank PIC";

                        int row = 2;
                        while (reader.Read())
                        {
                            worksheet.Cell(row, 1).Value = reader["No"].ToString(); // Convert to string
                            worksheet.Cell(row, 2).Value = reader["CustomerCode"].ToString();
                            worksheet.Cell(row, 3).Value = reader["TransactionType"].ToString();
                            worksheet.Cell(row, 4).Value = reader["Currency"].ToString();
                            worksheet.Cell(row, 5).Value = Convert.ToDecimal(reader["Amount"]); // Convert to decimal
                            worksheet.Cell(row, 6).Value = Convert.ToDateTime(reader["TransactionDate"]); // Convert to DateTime
                            worksheet.Cell(row, 7).Value = reader["BankPIC"].ToString();
                            row++;
                        }

                        workbook.SaveAs(filePath);
                        Console.WriteLine($"Data exported successfully to {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}