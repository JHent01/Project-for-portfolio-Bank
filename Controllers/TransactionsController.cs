using Bank.Models;
using Bank.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bank.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly string _connectionString = "User Id=sqlserver;Password=gfhjkm;Server=34.67.31.84;Database=Bank_Users;TrustServerCertificate=True;";
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
                        HttpContext.Session.SetString("UserName", users.UserName);
                        HttpContext.Session.SetString("Email", users.Email);
                        HttpContext.Session.SetString("PhoneNumber", users.PhoneNumber.ToString());
                        HttpContext.Session.SetString("Address", users.Address);
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
        //BankPortfolio
        public IActionResult Transfer()
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
                SqlCommand InfoFriends = new SqlCommand("SELECT user_cards FROM UserFriends WHERE Logins = @Logins", connection);
                InfoFriends.Parameters.AddWithValue("@Logins", log);
                using (SqlDataReader reader = InfoFriends.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var friendsList = new List<Friends>();
                        var json = reader.GetString(0);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var friends = System.Text.Json.JsonSerializer.Deserialize<List<Friends>>(json);
                            if (friends != null)
                            {
                                friendsList.AddRange(friends);
                            }
                        }
                        ViewBag.Friends = friendsList;
                    }
                    else
                    {
                        ViewBag.Friends = null;
                    }

                   
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Holl");
            }



















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
                    decimal bal = balansTake != null ? Convert.ToDecimal(balansTake) : 0;
                    using (SqlCommand transactionCommand = new SqlCommand("INSERT INTO Transactions (TransactionType, Amount, TransactionDate, NumberAccount, BalanceAfter) VALUES (@TransactionType, @Amount, @TransactionDate, @NumberAccount, @BalanceAfter)", connection))
                    {
                        transactionCommand.Parameters.AddWithValue("@TransactionType", TransactionType.Transfer);
                        transactionCommand.Parameters.AddWithValue("@Amount", summ);
                        transactionCommand.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
                        transactionCommand.Parameters.AddWithValue("@NumberAccount", NumberAccount);
                        transactionCommand.Parameters.AddWithValue("@BalanceAfter", bal );
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





        public IActionResult Changes()
        {
            ViewBag.CardExpiration = HttpContext.Session.GetString("CardExpiration");
            ViewBag.NumberAccount = HttpContext.Session.GetString("NumberAccount");
            ViewBag.Balance = HttpContext.Session.GetString("Balance");
            ViewBag.userInfo = HttpContext.Session.GetString("userInfo");

            ViewBag.Login = HttpContext.Session.GetString("Login");
            ViewBag.Email = HttpContext.Session.GetString("Email");
            ViewBag.PhoneNumber = HttpContext.Session.GetString("PhoneNumber");
            ViewBag.Address = HttpContext.Session.GetString("Address");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View();
        }

        [HttpPost]
        public IActionResult Changes(string? name,string? Email, long? PhoneNumber, string? Address,string? Login, string? Password)
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
                SqlCommand cmdCheckLogin = new SqlCommand("SELECT COUNT(*) FROM UserLog WHERE Login = @Login", connection);
                cmdCheckLogin.Parameters.AddWithValue("@Login", Login ?? string.Empty);
                int existingLoginCount = (int)cmdCheckLogin.ExecuteScalar();
                if (existingLoginCount > 0 && Login != null && Login != log)
                {
                    TempData["Error"] = "Логін вже використовується. Виберіть інший.";
                    return RedirectToAction("Holl");
                }
                else
                {

                    switch (Password, Login)
                    {
                        case (not null, not null):
                            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
                            SqlCommand cmd = new SqlCommand("UPDATE UserLog SET Password = @Password, Login = @Login WHERE Login = @OldLogin", connection);
                            cmd.Parameters.AddWithValue("@OldLogin", log);
                            cmd.Parameters.AddWithValue("@Password", hashedPassword);
                            cmd.Parameters.AddWithValue("@Login", Login);
                            cmd.ExecuteNonQuery();
                            HttpContext.Session.SetString("Login", Login);
                            break;
                        case (not null, null):
                            string hashedPasswords = BCrypt.Net.BCrypt.HashPassword(Password);
                            SqlCommand cmd1 = new SqlCommand("UPDATE UserLog SET Password = @Password WHERE Login = @OldLogin", connection);
                            cmd1.Parameters.AddWithValue("@OldLogin", log);
                            cmd1.Parameters.AddWithValue("@Password", hashedPasswords);
                            cmd1.ExecuteNonQuery();
                            break;
                        case (null, not null):
                            SqlCommand cmd2 = new SqlCommand("UPDATE UserLog SET Login = @Login WHERE Login = @OldLogin", connection);
                            cmd2.Parameters.AddWithValue("@OldLogin", log);
                            cmd2.Parameters.AddWithValue("@Login", Login);
                            cmd2.ExecuteNonQuery();
                            HttpContext.Session.SetString("Login", Login);
                            break;
                        case (null, null):
                            break;
                    }
                }
                switch (Email, PhoneNumber, Address)
                {
                    case (not null, not null, not null):
                        SqlCommand changeCommand = new SqlCommand("UPDATE UsersInfo SET Email = @Email, PhoneNumber = @PhoneNumber, Address = @Address WHERE UserId = (SELECT UserId FROM UserLog WHERE Login = @Login)", connection);
                        changeCommand.Parameters.AddWithValue("@Login", log);
                        changeCommand.Parameters.AddWithValue("@Email", Email);
                        changeCommand.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                        changeCommand.Parameters.AddWithValue("@Address", Address);
                        changeCommand.ExecuteNonQuery();
                        break;
                    case (not null, not null, null):
                        SqlCommand changeCommand1 = new SqlCommand("UPDATE UsersInfo SET Email = @Email, PhoneNumber = @PhoneNumber WHERE UserId = (SELECT UserId FROM UserLog WHERE Login = @Login)", connection);
                        changeCommand1.Parameters.AddWithValue("@Login", log);
                        changeCommand1.Parameters.AddWithValue("@Email", Email);
                        changeCommand1.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                        changeCommand1.ExecuteNonQuery();
                        break;
                    case (not null, null, not null):
                        SqlCommand changeCommand2 = new SqlCommand("UPDATE UsersInfo SET Email = @Email, Address = @Address WHERE UserId = (SELECT UserId FROM UserLog WHERE Login = @Login)", connection);
                        changeCommand2.Parameters.AddWithValue("@Login", log);
                        changeCommand2.Parameters.AddWithValue("@Email", Email);
                        changeCommand2.Parameters.AddWithValue("@Address", Address);
                        changeCommand2.ExecuteNonQuery();
                        break;
                    case (null, not null, not null):
                        SqlCommand changeCommand3 = new SqlCommand("UPDATE UsersInfo SET PhoneNumber = @PhoneNumber, Address = @Address WHERE UserId = (SELECT UserId FROM UserLog WHERE Login = @Login)", connection);
                        changeCommand3.Parameters.AddWithValue("@Login", log);
                        changeCommand3.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                        changeCommand3.Parameters.AddWithValue("@Address", Address);
                        changeCommand3.ExecuteNonQuery();
                        break;
                    case (not null, null, null):
                        SqlCommand changeCommand4 = new SqlCommand("UPDATE UsersInfo SET Email = @Email WHERE UserId = (SELECT UserId FROM UserLog WHERE Login = @Login)", connection);
                        changeCommand4.Parameters.AddWithValue("@Login", log);
                        changeCommand4.Parameters.AddWithValue("@Email", Email);
                        changeCommand4.ExecuteNonQuery();

                        break;
                case (null, not null, null):
                        SqlCommand changeCommand5 = new SqlCommand("UPDATE UsersInfo SET PhoneNumber = @PhoneNumber WHERE UserId = (SELECT UserId FROM UserLog WHERE Login = @Login)", connection);
                        changeCommand5.Parameters.AddWithValue("@Login", log);
                        changeCommand5.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                        changeCommand5.ExecuteNonQuery();
                        break;
                    case (null, null, not null):
                        SqlCommand changeCommand6 = new SqlCommand("UPDATE UsersInfo SET Address = @Address WHERE UserId = (SELECT UserId FROM UserLog WHERE Login = @Login)", connection);
                        changeCommand6.Parameters.AddWithValue("@Login", log);
                        changeCommand6.Parameters.AddWithValue("@Address", Address);
                        changeCommand6.ExecuteNonQuery();
                        break;
                    case (null, null, null):
                        break;
                }


                if (name != null)
                {
                        SqlCommand sqlCommand = new SqlCommand("UPDATE UsersInfo SET UserName = @UserName WHERE UserId = (SELECT UserId FROM UserLog WHERE Login = @Login)", connection);
                        sqlCommand.Parameters.AddWithValue("@Login", log);
                        sqlCommand.Parameters.AddWithValue("@UserName", name);
                        sqlCommand.ExecuteNonQuery();
                        HttpContext.Session.SetString("userInfo", name);
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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Friends()
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
            SqlCommand InfoFriends = new SqlCommand("SELECT user_cards FROM UserFriends WHERE Logins = @Logins", connection);
            InfoFriends.Parameters.AddWithValue("@Logins", log);
            using (SqlDataReader reader = InfoFriends.ExecuteReader())
            {
                if (reader.Read())
                {
                    var friendsList = new List<Friends>();
                    var json = reader.GetString(0);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var friends = System.Text.Json.JsonSerializer.Deserialize<List<Friends>>(json);
                        if (friends != null)
                        {
                            friendsList.AddRange(friends);
                        }
                    }
                    ViewBag.Friends = friendsList;
                }
                else
                {
                    ViewBag.Friends = null;
                }
            }

            ViewBag.CardExpiration = HttpContext.Session.GetString("CardExpiration");
            ViewBag.NumberAccount = HttpContext.Session.GetString("NumberAccount");
            ViewBag.Balance = HttpContext.Session.GetString("Balance");
            ViewBag.userInfo = HttpContext.Session.GetString("userInfo");
            return View();
        }

        [HttpPost]
        public IActionResult Friends(string? loginF)
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


                var comand = new SqlCommand("SELECT COUNT(*) FROM AccountCard WHERE Login = Login", connection);
                comand.Parameters.AddWithValue("@Login", loginF);

                int countBalanse = (int)comand.ExecuteScalar();
                if (countBalanse > 0)
                {
                    SqlCommand addLog = new SqlCommand("SELECT COUNT(*) FROM UserFriends WHERE Logins = @Login", connection);
                    addLog.Parameters.AddWithValue("@Login", log);
                    int countLog = (int)addLog.ExecuteScalar();
                    if (countLog == 0)
                    {
                        SqlCommand createCmd = new SqlCommand("INSERT INTO UserFriends (Logins, user_cards) VALUES (@Logins, '[]')", connection);
                        createCmd.Parameters.AddWithValue("@Logins", log);
                        createCmd.ExecuteNonQuery();
                    }

                    SqlCommand command = new SqlCommand("SELECT NumberAccount FROM AccountCard WHERE Login = @Login", connection);
                    command.Parameters.AddWithValue("@Login", loginF);
                    object result = command.ExecuteScalar();
                    string numberA = result?.ToString();
                    Friends friends = new Friends
                    (
                        Login: loginF,
                        NumberCard: numberA
                    );

                    if (loginF == log)
                    {
                        TempData["Error"] = "Ви не можете додати себе в друзі.";
                        return RedirectToAction("Friends");
                    }
                    
                    SqlCommand cmdCheckFriend = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM UserFriends 
                        CROSS APPLY OPENJSON(user_cards) 
                        WITH (Login nvarchar(100) '$.Login') as f
                        WHERE Logins = @Logins AND f.Login = @Login", connection);
                    cmdCheckFriend.Parameters.AddWithValue("@Logins", log);
                    cmdCheckFriend.Parameters.AddWithValue("@Login", friends.Login);

                    int existingFriendCount = (int)cmdCheckFriend.ExecuteScalar();
                    if (existingFriendCount > 0)
                    {
                        TempData["Error"] = "Цей користувач вже доданий в друзі.";
                        return RedirectToAction("Friends");
                    }
                    
                    SqlCommand addFriendCommand = new SqlCommand("UPDATE UserFriends SET user_cards = JSON_MODIFY(user_cards, 'append $', JSON_QUERY(@newData)) WHERE Logins = @Logins", connection);
                    
                    var newData = $"{{\"Login\": \"{friends.Login}\", \"NumberCard\": \"{friends.NumberCard}\"}}";
                    addFriendCommand.Parameters.AddWithValue("@Logins", log);
                    addFriendCommand.Parameters.AddWithValue("@newData", newData);
                    addFriendCommand.ExecuteNonQuery();

                    connection.Close();
                   
                    return RedirectToAction("Friends");
                }
                else
                {
                    connection.Close();
                    TempData["Error"] = "Користувача з таким логіном не існує.";
                    return RedirectToAction("Friends");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Holl");
            }

              
        }

        [HttpPost]
        public IActionResult DeleteFriend(string? logDel)
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
                SqlCommand cmdCheckFriend = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM UserFriends 
                        CROSS APPLY OPENJSON(user_cards) 
                        WITH (Login nvarchar(100) '$.Login') as f
                        WHERE Logins = @Logins AND f.Login = @Login", connection);
                cmdCheckFriend.Parameters.AddWithValue("@Logins", log);
                cmdCheckFriend.Parameters.AddWithValue("@Login", logDel);

                int existingFriendCount = (int)cmdCheckFriend.ExecuteScalar();
                if (existingFriendCount > 0)
                {
                    // Исправленный SQL-запрос для удаления друга из JSON-массива user_cards
                    SqlCommand deletFriends = new SqlCommand(@"
                        UPDATE UserFriends 
                        SET user_cards = (
                            SELECT 
                                CASE 
                                    WHEN COUNT(*) = 0 THEN '[]'
                                    ELSE 
                                        '[' + STRING_AGG(
                                            CONCAT(
                                                '{""Login"":""', Login, '"",""NumberCard"":""', NumberCard, '""}'
                                            ), 
                                            ','
                                        ) + ']'
                                END
                            FROM (
                                SELECT Login, NumberCard
                                FROM OPENJSON(user_cards)
                                WITH (
                                    Login nvarchar(100) '$.Login',
                                    NumberCard nvarchar(100) '$.NumberCard'
                                )
                                WHERE Login <> @Login
                            ) as friends
                        )
                        WHERE Logins = @Logins", connection);
                    deletFriends.Parameters.AddWithValue("@Logins", log);
                    deletFriends.Parameters.AddWithValue("@Login", logDel);
                    
                    
                    deletFriends.ExecuteNonQuery();
                    connection.Close();



                }
                else
                {
                    connection.Close();
                    TempData["Error"] = "Користувача з таким логіном не існує в друзях.";
                    return RedirectToAction("Holl");
                }


            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Holl");
            }




            return RedirectToAction("Friends");
        }
    }
}
