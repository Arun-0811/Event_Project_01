using FluentAssertions.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Session;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DevEventConnection");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login/Index"; // or wherever your login page is
        options.AccessDeniedPath = "/Login/Login/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireClaim("Role", "Admin"));
    options.AddPolicy("CustomerPolicy", policy =>
        policy.RequireClaim("Role", "Customer"));
});

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // how long session time set
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add authentication
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

// Add authorization
builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Customer/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseSession(); // Enable session middleware

// Custom middleware to restrict access to admin/customer pages
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    // Define routes to protect
    if (path.Contains("/admin") || path.Contains("/customer"))
    {
        // Check if the session exists
        var userSession = context.Session.GetString("my-session");
        if (string.IsNullOrEmpty(userSession))
        {
            // Redirect to login if session is invalid
            context.Response.Redirect("/Login/Index");
            return;
        }
    }
    await next();
});


app.UseHttpsRedirection();
app.UseRouting();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map static assets (like images, CSS, JS, etc.)
app.MapStaticAssets();

// Default route for normal (non-area) controllers (this should be last)
app.MapControllerRoute(
    name: "Guest",
    pattern: "{area:exists}/{controller=Guest}/{action=GuestIndex}/{id?}");

// Area-specific routes first (correct order)
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=EventAdmin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "customer",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "login",
    pattern: "{area:exists}/{controller=Login}/{action=Index}/{id?}");



// Enable static files (CSS, JS, etc.)
app.UseStaticFiles();

// Run the application
app.Run();
