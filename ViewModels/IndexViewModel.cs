using Bank.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Bank.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<UserLog> UserLog { get; set; } = new List<UserLog>();
        public IEnumerable<UserInfo> UserInfo { get; set; } = new List<UserInfo>();
        public IEnumerable<AccountCard> AccountCard { get; set; } = new List<AccountCard>();

       
    }
}
