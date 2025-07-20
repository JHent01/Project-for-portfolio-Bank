using Microsoft.AspNetCore.Mvc;
using Bank.Models;
namespace Bank.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<UserLog> UserLog { get; set; } = new List<UserLog>();
        public IEnumerable<UserInfo> UserInfo { get; set; } = new List<UserInfo>();

    }
}
