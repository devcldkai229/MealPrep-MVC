using MealPrep.BLL.Extensions;
using MealPrep.DAL.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Amazon.S3;
using Amazon;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDalServices(builder.Configuration);

// Configure AWS S3 Client (uses default credentials from environment/IAM role)
var awsRegion = builder.Configuration["AwsS3:Region"] ?? "ap-northeast-1";
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(awsRegion)
    };
    return new AmazonS3Client(config); // Uses default credential chain (IAM role, environment variables, etc.)
});

builder.Services.AddBllServices();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Auth/Login";
        o.AccessDeniedPath = "/Auth/AccessDenied";
    });

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
