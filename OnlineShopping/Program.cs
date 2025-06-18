using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnlineShopping.Data;
using OnlineShopping.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
//    Add Services
// ----------------------
builder.Services.AddControllersWithViews();

//  Configure DbContext
builder.Services.AddDbContext<OnlineShoppingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OnlineShoppingContext")));

//  Configure Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//  Configure Authentication using Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.AccessDeniedPath = "/User/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
    });

//  Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

//  Register CartService in DI container
builder.Services.AddSingleton<CartService>(); // Or AddScoped<CartService>() if you want a new instance per request

var app = builder.Build();

// ----------------------
//    Configure Pipeline
// ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//  Important: Authentication ➤ Session ➤ Authorization
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

//  Set Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=ProductDashboard}/{id?}");

app.Run();
