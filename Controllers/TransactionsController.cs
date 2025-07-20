using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Bank.Controllers
{
    public class TransactionsController : Controller
    { private readonly string _connectionString = "Data Source=localhost;Initial Catalog=Bank_Users;Integrated Security=True; TrustServerCertificate=True";

        public IActionResult Holl(string login)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                return View("Error", "Database connection failed.");// ошибка подключения к базе данных   
            }

            return View(login);

        }
    }
}
