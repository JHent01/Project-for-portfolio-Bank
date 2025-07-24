var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // ����� ����� ������
    options.Cookie.HttpOnly = true; // ������ �� XSS
    options.Cookie.IsEssential = true; // ������������ ���� (��� GDPR)
});



var app = builder.Build();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");



app.Run();

//ConnectionStrings:DatabaseConnection