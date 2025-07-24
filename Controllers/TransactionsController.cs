using Bank.Models;
using Bank.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bank.Controllers
{
    public class TransactionsController : Controller
    { private readonly string _connectionString = "Data Source=localhost;Initial Catalog=Bank_Users;Integrated Security=True; TrustServerCertificate=True";
        
        public IActionResult Holl()
        {   try
            {
                var log = HttpContext.Session.GetString("Login");
               
               
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    return View("Error", "Database connection failed.");// ошибка подключения к базе данных   
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
                        ViewBag.CardExpiration =reader.GetString(0);// тут оно на англ( не знаю надо будет менять в числовой формат или оставить так, подумать потом)
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
                TempData["Balance"] = balance;
                HttpContext.Session.SetString("Balance", balance);
                
                connection.Close();
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", $"An error occurred: {ex.Message}");
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
                    return View("Error", "User not logged in.");
                }
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    return View("Error", "Database connection failed.");// ошибка подключения к базе данных   
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
                        return View("Error", "Insufficient funds for the transfer.");
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
                }
                else
                {                    connection.Close();
                    return View("Error", "Invalid account number.");
                }
                connection.Close();
               
                return RedirectToAction("Holl");
            }
            catch (Exception ex)
            {
                return View("Error", $"An error occurred: {ex.Message}");
            }
        }
    }
}
