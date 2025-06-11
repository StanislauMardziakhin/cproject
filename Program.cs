using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CourseProject.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? Environment.GetEnvironmentVariable("DATABASE_URL");
builder.Services.AddDbContext<CourseProject.Models.AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<CourseProject.Models.ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CourseProject.Models.AppDbContext>()
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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataInitializer.SeedRolesAndAdmin(services);
}

app.Run();