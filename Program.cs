using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"DATABASE_URL: {(string.IsNullOrEmpty(databaseUrl) ? "null or empty" : databaseUrl[..10] + "****" + (databaseUrl.IndexOf('@') > 0 ? databaseUrl[databaseUrl.IndexOf('@')..] : "invalid"))}");
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"DefaultConnection: {(string.IsNullOrEmpty(defaultConnection) ? "null or empty" : defaultConnection)}");
var connectionString = databaseUrl ?? defaultConnection;
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string is missing or empty. Ensure DATABASE_URL or DefaultConnection is set.");
}
var logSafeConnectionString = connectionString;
var passwordStart = connectionString.IndexOf(':') + 1;
var passwordEnd = connectionString.IndexOf('@');
if (passwordEnd > passwordStart)
{
    logSafeConnectionString = connectionString[..passwordStart] + "****" + connectionString[passwordEnd..];
}
Console.WriteLine($"Using connection string: {logSafeConnectionString}");
if (connectionString.StartsWith("postgresql://"))
{
    connectionString = connectionString.Replace("postgresql://", "postgres://");
    Console.WriteLine($"Converted connection string: {logSafeConnectionString.Replace("postgresql://", "postgres://")}");
}

builder.Services.AddDbContext<CourseProject.Models.AppDbContext>(options =>
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