
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.Json;
using CamControl.Services;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CamControl.Constraints;
using CamControl;
using CamControl.Hubs;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)

    .ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("CameraConfig.json",
                        optional: true,
                        reloadOnChange: true);
    config.AddJsonFile("SettingsConfig.json",
                        optional: true,
                        reloadOnChange: true); 
}).ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.UseStartup<Startup>();
});
    
}

//var builder = WebApplication.CreateBuilder(args);
//builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
//{
//    config.AddJsonFile("CameraConfig.json",
//                        optional: true,
//                        reloadOnChange: true); ;
//});
//// Add services to the container.
//builder.Services
//    .AddRazorPages()
//    .AddViewLocalization()
//    .AddDataAnnotationsLocalization();
//builder.Services.Configure<RouteOptions>(options => { options.ConstraintMap.Add("cameraid", typeof(CameraRouteConstraint)); });
////builder.Services.AddControllersWithViews()
////    .AddRazorOptions(options =>
////    {
////        options.ViewLocationFormats.Add("/{0}.cshtml");
////    });
////builder.Services.AddMvc().AddViewLocalization();
//builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
//builder.Services.AddMvc()
//        .AddViewLocalization(
//            LanguageViewLocationExpanderFormat.Suffix,
//            opts => { opts.ResourcesPath = "Resources"; })
//        .AddDataAnnotationsLocalization();


//builder.Services.AddSingleton(typeof(ICameraService), typeof(CameraService));
//builder.Services.Configure<RequestLocalizationOptions>(options =>
//{
//    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("de") };
//    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("de-DE");
//    options.SupportedCultures = supportedCultures;
//    options.SupportedUICultures = supportedCultures;
//    options.RequestCultureProviders = new List<IRequestCultureProvider>
//{
//    new QueryStringRequestCultureProvider(),
//    new CookieRequestCultureProvider()
//};
//});
//builder.Services.Configure<CookiePolicyOptions>(options =>
//{
//    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
//    options.CheckConsentNeeded = context => true;
//    options.MinimumSameSitePolicy = SameSiteMode.None;
//});
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
//    // Add more policies as needed
//});
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptions => {
//    cookieOptions.Cookie.HttpOnly = true;
//    cookieOptions.LoginPath = "/Home/Login";
//    cookieOptions.LogoutPath = "/Home/Logout";
//    cookieOptions.AccessDeniedPath= "/Forbidden";
//});
////builder.Services.ConfigureApplicationCookie(options =>
////{
////    options.LoginPath = "/Home/Login";
////    options.LogoutPath = "/Home/Logout";
////});
////builder.Services.AddRazorPages(options =>
//// {
////     options.Conventions.AuthorizeFolder("/admin", "AdminOnly");
//// });

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}
//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseAuthorization();
//app.UseAuthentication();
//app.UseCookiePolicy();
//var options = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();

//app.UseRequestLocalization(options.Value);
//app.MapRazorPages();

//app.Run();
