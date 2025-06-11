using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string is missing or empty. Ensure DATABASE_URL or DefaultConnection is set.");
}
var logSafeConnectionString = connectionString;
var atIndex = connectionString.IndexOf('@');
if (atIndex > 10)
{
    logSafeConnectionString = connectionString[..10] + "****" + connectionString[atIndex..];
}
Console.WriteLine($"Using connection string: {logSafeConnectionString}");
if (connectionString.StartsWith("postgresql://"))
    connectionString = connectionString.Replace("postgresql://", "postgres://");
Console.WriteLine($"Converted connection string: {logSafeConnectionString.Replace("postgresql://", "postgres://")}");

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
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    await DataInitializer.SeedRolesAndAdmin(scope.ServiceProvider);
}

app.Run();