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
        private readonly string _connectionString = "DATABASE_URL";
        //Data Source=localhost;Initial Catalog=Bank_Users;Integrated Security=True; TrustServerCertificate=True   strong_password
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
        {
            if (user == null || string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
            {
                TempData["Error"] = "Логін чи пароль пусті";
                return View("index");
               
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);

                connection.Open();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    TempData["Error"] = "Помилка підключення бази данних";
                    return View("index");
                }

                SqlCommand command = new SqlCommand("SELECT * FROM UserLog WHERE Login = @Login", connection);
                command.Parameters.AddWithValue("@Login", user.Login);



                var userCount = command.ExecuteScalar();
                if (userCount == null)
                {
                    connection.Close();
                    TempData["Error"] = "Неправильні данні";
                    
                    return View("Registration");

                }
                else if (Login(user))
                {

                    connection.Close();


                    HttpContext.Session.SetString("Login", user.Login);
                    return RedirectToAction("Holl", "Transactions");
                }
                else
                {
                    connection.Close();
                    TempData["Error"] = "Проль неправильний";
                    return View("index");
                }



            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return View("index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("index");
              
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
                if (userCount > 0)
                {
                    TempData["Error"] = "Користувач з такім логіном вже існує";
                    return View("index"); ;
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
                    HttpContext.Session.SetString("Login", user.Login);
                    return RedirectToAction("Holl", "Transactions");// ++



                }

            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
                return View("index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("index");
            }
        }

        
        public IActionResult InfoBank()
        {

            return View();
        }
        public IActionResult Contacts()
        {

            return View();
        }
    }
}


    
