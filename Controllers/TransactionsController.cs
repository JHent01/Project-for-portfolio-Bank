using Bank.Models;
using Bank.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bank.Controllers
{//Server=176.37.174.35,1433;Database=Bank_Users;User Id=User;Password=qwerty;TrustServerCertificate=True;Connect Timeout=30;
    public class TransactionsController : Controller
    { private readonly string _connectionString = "DATABASE_URL";
        public IActionResult Information()
        {
            try
            {
                ViewBag.CardExpiration = HttpContext.Session.GetString("CardExpiration");
                ViewBag.NumberAccount = HttpContext.Session.GetString("NumberAccount");
                ViewBag.Balance = HttpContext.Session.GetString("Balance");
                ViewBag.userInfo = HttpContext.Session.GetString("userInfo");
                ViewBag.Login = HttpContext.Session.GetString("Login");
                var log = HttpContext.Session.GetString("Login");
                if (string.IsNullOrEmpty(log))
                {
                    TempData["Error"] = "Час сесії вийшов. Авторизуйтесь заново ";
                    return RedirectToAction("Holl");
                }
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    TempData["Error"] = "Помилка підключення бази данних";
                    return RedirectToAction("Holl");// ошибка подключения к базе данных   
                }
                var cmd = new SqlCommand(
                           "SELECT UserId FROM UserLog WHERE Login = @Login",
                               connection);
                cmd.Parameters.AddWithValue("@Login", log);
                object result = cmd.ExecuteScalar();
                var id = result?.ToString();
                SqlCommand command = new SqlCommand("SELECT UserName,Email,PhoneNumber,Address FROM UsersInfo WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", id);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    
                        
                        if (reader.Read())
                        {
                            UserInfo users = new UserInfo
                            (
                                UserName: reader.GetString(0),
                                Email: reader.GetString(1),
                                PhoneNumber: long.Parse(reader.GetString(2)),
                                Address: reader.GetString(3)
                            );

                            ViewBag.userInformation = users;
                            
                        }



                    connection.Close(); 


                }
                
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Holl");
            }
           
            return View();
        }
        public IActionResult Holl()
        {   try
            {
                var log = HttpContext.Session.GetString("Login");
               
               
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    TempData["Error"] = "Помилка підключення бази данних";
                    return RedirectToAction("Holl");// ошибка подключения к базе данных   
                }
                var cmd = new SqlCommand(
                           "SELECT UserId FROM UserLog WHERE Login = @Login",
                               connection);
                cmd.Parameters.AddWithValue("@Login", log);

                object result = cmd.ExecuteScalar();
                var id = result?.ToString();

                SqlCommand command = new SqlCommand("SELECT UserName,Email,PhoneNumber,Address FROM UsersInfo WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", id);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {

                        ViewBag.userInfo = reader.GetString(0);
                        
                        HttpContext.Session.SetString("userInfo", reader.GetString(0));

                    }
                    

                }
                SqlCommand cartInf = new SqlCommand("SELECT CardExpiration,NumberAccount FROM AccountCard WHERE Login = @Login", connection);
                cartInf.Parameters.AddWithValue("@Login", log);
                string Numb = "";
                using (SqlDataReader reader = cartInf.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ViewBag.CardExpiration =reader.GetString(0);
                        Numb = reader.GetString(1);
                        ViewBag.NumberAccount = Numb;
                        HttpContext.Session.SetString("NumberAccount", Numb);
                        HttpContext.Session.SetString("CardExpiration", reader.GetString(0));
                        
                    }
                }
                SqlCommand balanceCommand = new SqlCommand("SELECT Balance FROM Balance WHERE NumberBalance = @NumberBalance", connection);
                balanceCommand.Parameters.AddWithValue("@NumberBalance", Numb);
                object results = balanceCommand.ExecuteScalar();
                string balance = results?.ToString();
                ViewBag.Balance = balance;
               
                HttpContext.Session.SetString("Balance", balance);
                
                connection.Close();
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Holl");
                
            }
        }
        
        public IActionResult Transfer()
        {
            ViewBag.CardExpiration = HttpContext.Session.GetString("CardExpiration"); 
            ViewBag.NumberAccount = HttpContext.Session.GetString("NumberAccount"); 
            ViewBag.Balance = HttpContext.Session.GetString("Balance"); 
            ViewBag.userInfo = HttpContext.Session.GetString("userInfo");
            return View();
        }
        [HttpPost]  
        public IActionResult Transfer(decimal? summ, string? NumberAccount)
        {
            try
            {
              
                var log = HttpContext.Session.GetString("Login");
                if (string.IsNullOrEmpty(log))
                {
                    TempData["Error"] = "Час сесії вийшов. Авторизуйтесь заново ";
                    return RedirectToAction("Holl");
                }
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    TempData["Error"] = "Помилка підключення бази данних";
                    return RedirectToAction("Holl");// ошибка подключения к базе данных   
                }

                SqlCommand command = new SqlCommand("SELECT NumberAccount FROM AccountCard WHERE Login = @Login", connection);
                command.Parameters.AddWithValue("@Login", log);
                object result = command.ExecuteScalar();
                string numberA = result?.ToString();
                var comand = new SqlCommand("SELECT COUNT(*) FROM Balance WHERE NumberBalance = @NumberBalance", connection);
                comand.Parameters.AddWithValue("@NumberBalance", NumberAccount);

                int countBalanse = (int)comand.ExecuteScalar();
                if (countBalanse > 0)
                {



                    SqlCommand balanceCommand = new SqlCommand("SELECT Balance FROM Balance WHERE NumberBalance = @NumberBalance", connection);
                    balanceCommand.Parameters.AddWithValue("@NumberBalance", numberA);
                    object balanceResult = balanceCommand.ExecuteScalar();
                    decimal balance = balanceResult != null ? Convert.ToDecimal(balanceResult) : 0;
                    if (balance < summ)
                    {
                        TempData["Error"] = "Недостатньо коштів для переказу";
                        return View("Transfer");
                    }

                    using (SqlCommand transferCommand = new SqlCommand("UPDATE Balance SET Balance = Balance - @Summ WHERE NumberBalance = @NumberBalance", connection))
                    {   transferCommand.Parameters.AddWithValue("@NumberBalance", numberA);
                        transferCommand.Parameters.AddWithValue("@Summ", summ);
                        transferCommand.ExecuteNonQuery();
                    }
                   
                    using (SqlCommand transferCommand2 = new SqlCommand("UPDATE Balance SET Balance = Balance + @Summ WHERE NumberBalance = @NumberBalance2", connection))
                    {
                        transferCommand2.Parameters.AddWithValue("@NumberBalance2", NumberAccount);
                        transferCommand2.Parameters.AddWithValue("@Summ", summ);
                        transferCommand2.ExecuteNonQuery();
                    }

                    using (SqlCommand transactionCommand = new SqlCommand("INSERT INTO Transactions (TransactionType, Amount, TransactionDate, NumberAccount, BalanceAfter) VALUES (@TransactionType, @Amount, @TransactionDate, @NumberAccount, @BalanceAfter)", connection))
                    {
                        transactionCommand.Parameters.AddWithValue("@TransactionType", TransactionType.Transfer);
                        transactionCommand.Parameters.AddWithValue("@Amount", summ);
                        transactionCommand.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
                        transactionCommand.Parameters.AddWithValue("@NumberAccount", numberA);
                        transactionCommand.Parameters.AddWithValue("@BalanceAfter", balance - summ);
                        transactionCommand.ExecuteNonQuery();
                    }
                    
                    
                    SqlCommand cmnd = new SqlCommand("SELECT Balance FROM Balance WHERE NumberBalance = @NumberBalance", connection);
                    cmnd.Parameters.AddWithValue("@NumberBalance", NumberAccount);
                    object balansTake = cmnd.ExecuteScalar();
                    decimal bal = balansTake != null ? Convert.ToDecimal(balanceResult) : 0;
                    using (SqlCommand transactionCommand = new SqlCommand("INSERT INTO Transactions (TransactionType, Amount, TransactionDate, NumberAccount, BalanceAfter) VALUES (@TransactionType, @Amount, @TransactionDate, @NumberAccount, @BalanceAfter)", connection))
                    {
                        transactionCommand.Parameters.AddWithValue("@TransactionType", TransactionType.Transfer);
                        transactionCommand.Parameters.AddWithValue("@Amount", summ);
                        transactionCommand.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
                        transactionCommand.Parameters.AddWithValue("@NumberAccount", NumberAccount);
                        transactionCommand.Parameters.AddWithValue("@BalanceAfter", bal + summ);
                        transactionCommand.ExecuteNonQuery();
                    }






                }
                else
                {                    connection.Close();
                    TempData["Error"] = "Недійсний номер рахунку.";
                    return RedirectToAction("Holl");
                   
                }
                connection.Close();
               
                return RedirectToAction("Holl");
            }
            catch (Exception ex)
            {
                TempData["Error"] =ex.Message;
                return RedirectToAction("Holl");
               
            }
        }
        public IActionResult TransactionHistory()
        {

            var log = HttpContext.Session.GetString("Login");
            if (string.IsNullOrEmpty(log))
            {
                TempData["Error"] = "Час сесії вийшов. Авторизуйтесь заново ";
                return RedirectToAction("Holl");
                
            }
            ViewBag.Login = log;
            ViewBag.CardExpiration = HttpContext.Session.GetString("CardExpiration");
            ViewBag.NumberAccount = HttpContext.Session.GetString("NumberAccount");
            ViewBag.Balance = HttpContext.Session.GetString("Balance");
            ViewBag.userInfo = HttpContext.Session.GetString("userInfo");
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                TempData["Error"] = "Помилка підключення бази данних";
                return RedirectToAction("Holl");// ошибка подключения к базе данных   
            }
            SqlCommand command = new SqlCommand("SELECT NumberAccount FROM AccountCard WHERE Login = @Login", connection);
            command.Parameters.AddWithValue("@Login", log);
            object result = command.ExecuteScalar();
            string NumberAccount = result?.ToString();

            SqlCommand transactionCommand = new SqlCommand("SELECT COUNT(*) FROM Transactions WHERE NumberAccount = @NumberAccount", connection);
            transactionCommand.Parameters.AddWithValue("@NumberAccount", NumberAccount);

            int countBalanse = (int)transactionCommand.ExecuteScalar();
            if (countBalanse > 0)
            {
                SqlCommand histortTrans = new SqlCommand("SELECT Id,TransactionType,Amount,TransactionDate,BalanceAfter FROM Transactions WHERE NumberAccount = @NumberAccount", connection);
                histortTrans.Parameters.AddWithValue("@NumberAccount", NumberAccount);
                using (SqlDataReader reader = histortTrans.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var transactions = new List<Transaction>();
                        do
                        {
                            TransactionType type = Enum.Parse<TransactionType>(reader.GetString(1));
                            transactions.Add(new Transaction(
                                Id: reader.GetInt32(0),
                                TransactionType: type,
                                Amount: reader.GetDecimal(2),
                                TransactionDate: reader.GetDateTime(3),
                                NumberAccount: NumberAccount,
                                BalanceAfterTransaction: reader.GetDecimal(4)
                            ));
                            ViewBag.Transactions = transactions;
                        } while (reader.Read());
                       
                        return View();
                        

                    }


                }


            }

                return View();
        }


        public IActionResult Replenishment()
        {

            ViewBag.CardExpiration = HttpContext.Session.GetString("CardExpiration");
            ViewBag.NumberAccount = HttpContext.Session.GetString("NumberAccount");
            ViewBag.Balance = HttpContext.Session.GetString("Balance");
            ViewBag.userInfo = HttpContext.Session.GetString("userInfo");

            return View();  
        }
        [HttpPost]
        public IActionResult Replenishment(decimal? summ)
        {
            try
            {
                var log = HttpContext.Session.GetString("Login");
                if (string.IsNullOrEmpty(log))
                {
                    TempData["Error"] = "Час сесії вийшов. Авторизуйтесь заново ";
                    return RedirectToAction("Holl");
                    
                }
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    TempData["Error"] = "Помилка підключення бази данних";
                    return RedirectToAction("Holl");
                    // ошибка подключения к базе данных   
                }
                SqlCommand command = new SqlCommand("SELECT NumberAccount FROM AccountCard WHERE Login = @Login", connection);
                command.Parameters.AddWithValue("@Login", log);
                object result = command.ExecuteScalar();
                string numberA = result?.ToString();
                SqlCommand balanceCommand = new SqlCommand("SELECT Balance FROM Balance WHERE NumberBalance = @NumberBalance", connection);
                balanceCommand.Parameters.AddWithValue("@NumberBalance", numberA);
                object balanceResult = balanceCommand.ExecuteScalar();
                decimal balance = balanceResult != null ? Convert.ToDecimal(balanceResult) : 0;
                using (SqlCommand transferCommand = new SqlCommand("UPDATE Balance SET Balance = Balance + @Summ WHERE NumberBalance = @NumberBalance", connection))
                {
                    transferCommand.Parameters.AddWithValue("@NumberBalance", numberA);
                    transferCommand.Parameters.AddWithValue("@Summ", summ);
                    transferCommand.ExecuteNonQuery();
                }
                using (SqlCommand transactionCommand = new SqlCommand("INSERT INTO Transactions (TransactionType, Amount, TransactionDate, NumberAccount, BalanceAfter) VALUES (@TransactionType, @Amount, @TransactionDate, @NumberAccount, @BalanceAfter)", connection))
                {
                    transactionCommand.Parameters.AddWithValue("@TransactionType", TransactionType.Income);
                    transactionCommand.Parameters.AddWithValue("@Amount", summ);
                    transactionCommand.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
                    transactionCommand.Parameters.AddWithValue("@NumberAccount", numberA);
                    transactionCommand.Parameters.AddWithValue("@BalanceAfter", balance + summ);
                    transactionCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Holl");
                
            }
            return RedirectToAction("Holl");
        }
    }
}
