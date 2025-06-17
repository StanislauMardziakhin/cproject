using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"Raw DATABASE_URL: {(string.IsNullOrEmpty(databaseUrl) ? "null or empty" : databaseUrl)}");
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine(
    $"DefaultConnection: {(string.IsNullOrEmpty(defaultConnection) ? "null or empty" : defaultConnection)}");

var connectionStringService = new ConnectionStringConverter(databaseUrl, defaultConnection);
var efConnectionString = connectionStringService.GetConnectionString();
Console.WriteLine($"Using connection string: {efConnectionString}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(efConnectionString));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options => { options.AccessDeniedPath = "/Home/Index"; });
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<AccountService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DataInitializer.SeedRolesAndAdmin(scope.ServiceProvider);
}

app.Run();