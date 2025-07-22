using Bank.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Data.SqlClient;
using NuGet.Protocol.Plugins;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;
using System.Windows.Input;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bank.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString = "Data Source=localhost;Initial Catalog=Bank_Users;Integrated Security=True; TrustServerCertificate=True";

        public bool Login(UserLog user)
        {
            

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var cmd = new SqlCommand(
               "SELECT Password FROM UserLog WHERE Login = @Login",
               connection);
            cmd.Parameters.AddWithValue("@Login", user.Login);

            object result = cmd.ExecuteScalar();
            string hash = result?.ToString();

            if (hash == null) return false;

            return BCrypt.Net.BCrypt.Verify(user.Password, hash);
                
        }
        public IActionResult Index()
        {
            
            return View();
        }
        
        [HttpPost]
        public IActionResult Index(UserLog user)
        {if (user == null || string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
            {
                return View("Error", "Login and password cannot be empty.");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                
                    connection.Open();
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        return View("Error", "Database connection failed.");// ошибка подключения к базе данных   
                     }

                SqlCommand command = new SqlCommand("SELECT * FROM UserLog WHERE Login = @Login", connection);              
                        command.Parameters.AddWithValue("@Login", user.Login);

                

                var userCount = command.ExecuteScalar();
                            if (userCount == null)
                            {
                             connection.Close();
                             return View("Registration");// неверные учетные данные входа

                            }
                            else if(Login(user))
                            {

                            connection.Close();
                    TempData["Login"]= user.Login;
                   
                    return RedirectToAction("Holl", "Transactions"); 
                        }
                            else
                            {
                            connection.Close();
                            return View("Error", "Invalid login or password.");// неверные пароль  входа
                            }

                            

            }
            catch (SqlException ex)
            {
                return View("Error", $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return View("Error", $"An unexpected error occurred: {ex.Message}");
            }

          

        }

        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Registration(UserLog user, UserInfo userInfo)
        {
            try
            {




                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                using var connect = new SqlConnection(_connectionString);
                connect.Open();

                var comand = new SqlCommand("SELECT COUNT(*) FROM UserLog WHERE Login = @Login", connect);
                comand.Parameters.AddWithValue("@Login", user.Login);
                
                int userCount = (int)comand.ExecuteScalar();
                if (userCount>0)
                {
                    return View("index");// пользователь с таким логином уже существует
                }
                else
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO UserLog(Login, Password) VALUES(@Login, @Password)", connect))
                    {
                        cmd.Parameters.AddWithValue("@Login", user.Login);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        
                        cmd.ExecuteNonQuery();
                        
                        

                    }
                    using (SqlCommand cmdInfo = new SqlCommand("INSERT INTO UsersInfo(UserId,UserName, Email, PhoneNumber, Address) VALUES( @UserId, @UserName, @Email, @PhoneNumber, @Address)", connect))
                    {
                        var cmd = new SqlCommand(
                        "SELECT UserId FROM UserLog WHERE Login = @Login",
                            connect);
                        cmd.Parameters.AddWithValue("@Login", user.Login);

                        object result = cmd.ExecuteScalar();
                        var id = result?.ToString();
                        cmdInfo.Parameters.AddWithValue("@UserId", id);
                        cmdInfo.Parameters.AddWithValue("@UserName", userInfo.UserName);
                        cmdInfo.Parameters.AddWithValue("@Email", userInfo.Email);
                        cmdInfo.Parameters.AddWithValue("@PhoneNumber", userInfo.PhoneNumber);
                        cmdInfo.Parameters.AddWithValue("@Address", userInfo.Address);
                        cmdInfo.ExecuteNonQuery();
                    }
                   


                    using (SqlCommand cmdCard = new SqlCommand("INSERT INTO AccountCard(Login,NumberAccount, CardExpiration) VALUES(@Login,@NumberAccount, @CardExpiration)", connect))
                    {
                        cmdCard.Parameters.AddWithValue("@Login", user.Login);
                        
                        cmdCard.Parameters.AddWithValue("@NumberAccount", new Random().Next(100000000, 999999999));
                        cmdCard.Parameters.AddWithValue("@CardExpiration", DateTime.Now.AddYears(5));
                        cmdCard.ExecuteNonQuery();
                    }
                  

                        connect.Close();
                    TempData["Login"] = user.Login;
                    return RedirectToAction("Holl", "Transactions");// успешный вход 



                }

            }
            catch (SqlException ex)
            {
                return View("Error", $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return View("Error", $"An unexpected error occurred: {ex.Message}");
            }
        }
        public string Error(Exception? Message)
        { 
            return $"ex {Message}"; 

        }


    }

}
