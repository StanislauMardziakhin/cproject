using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"Raw DATABASE_URL: {(string.IsNullOrEmpty(databaseUrl) ? "null or empty" : databaseUrl)}");
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"DefaultConnection: {(string.IsNullOrEmpty(defaultConnection) ? "null or empty" : defaultConnection)}");
var connectionString = databaseUrl ?? defaultConnection;
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string is missing or empty. Ensure DATABASE_URL or DefaultConnection is set.");
}
Console.WriteLine($"Using connection string: {connectionString}");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await DataInitializer.SeedRolesAndAdmin(scope.ServiceProvider);
}

app.Run();