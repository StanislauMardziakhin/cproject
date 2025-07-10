using System.Globalization;
using CourseProject.Hubs;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SharedResources = CourseProject.Resources.SharedResources;

var builder = WebApplication.CreateBuilder(args);
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionStringService = new ConnectionStringConverter(databaseUrl, defaultConnection);
var efConnectionString = connectionStringService.GetConnectionString();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(efConnectionString));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options => { options.AccessDeniedPath = "/Home/Index"; });
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<TemplateService>();
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<FormService>();
builder.Services.AddScoped<IFormResultService, FormResultService>();
builder.Services.AddScoped<IResultsAggregatorService, ResultsAggregatorService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<LikeService>();
builder.Services.AddScoped<TemplateAccessService>();
builder.Services.AddScoped<TemplatePublicationService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<TemplateSearchService>();
builder.Services.AddScoped<SalesforceService>();
builder.Services.AddSignalR();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResources));
    });
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("es") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders =
    [
        new CookieRequestCultureProvider { CookieName = "UserCulture" },
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ]
});

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
app.MapHub<CommentHub>("/commentHub");
app.MapHub<LikeHub>("/likeHub");

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