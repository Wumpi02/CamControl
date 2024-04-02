using CamControl.Constraints;
using CamControl.Hubs;
using CamControl.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;

namespace CamControl
{
	public class Startup
	{
		public IWebHostEnvironment HostingEnvironment { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages()
			
			.AddViewLocalization()
			.AddDataAnnotationsLocalization();
			services.AddSignalR(hubOptions => { hubOptions.EnableDetailedErrors = true; });
            services.AddLocalization(options => options.ResourcesPath = "Resources");
			services.AddMvc()
		.AddViewLocalization(
			LanguageViewLocationExpanderFormat.Suffix,
			opts => { opts.ResourcesPath = "Resources"; })
		.AddDataAnnotationsLocalization();
            services.AddSingleton(typeof(ISettingsService), typeof(SettingsService));
            services.AddSingleton(typeof(ICameraService), typeof(CameraService));
            services.AddSingleton(typeof(IObsService), typeof(ObsService));



            services.Configure<RequestLocalizationOptions>(options =>
			{
				var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("de") };
				options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("de-DE");
				options.SupportedCultures = supportedCultures;
				options.SupportedUICultures = supportedCultures;
				options.RequestCultureProviders = new List<IRequestCultureProvider>
{
	new QueryStringRequestCultureProvider(),
	new CookieRequestCultureProvider()
};
			});

			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});


			services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptions =>
			{
				cookieOptions.LoginPath = "/Home/Login";
               

                cookieOptions.LogoutPath = "/Home/logout";
                
                cookieOptions.Cookie.HttpOnly = true; // Ensure that the cookie is accessible only over HTTP
                cookieOptions.Cookie.SameSite = SameSiteMode.Strict; // Set the same-site policy for the cookie
                cookieOptions.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Set the expiration time for the cookie
                cookieOptions.AccessDeniedPath = "/Forbidden/";
			});
			services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
				// Add more policies as needed
			});
			services.AddRazorPages(options =>
			{
				options.Conventions.AuthorizeFolder("/admin");
                options.Conventions.AddPageRoute("/Main/Index", "");
            });
			services.Configure<RouteOptions>(options => { 
				options.ConstraintMap.Add("cameraguid", typeof(CameraRouteConstraint));
                options.ConstraintMap.Add("deviceguid", typeof(DeviceRouteConstraint));
            });

		

        }

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<RequestLocalizationOptions> options)
		{
			// Configure the HTTP request pipeline.
			if (!env.IsDevelopment())
			{
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			
			
			app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCookiePolicy();
            app.UseStatusCodePages();
            app.Use(async (context, next) =>
			{
				var cookies = context.Request.Cookies;
				await next.Invoke();
			});
			//var options = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();

			app.UseRequestLocalization(options.Value);
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapRazorPages();
				endpoints.MapHub<ApplicationHub>("/obsHub");
			});


			IObsService obsService = app.ApplicationServices.GetService<IObsService>();
			obsService.Connect();



        }
    }
}
