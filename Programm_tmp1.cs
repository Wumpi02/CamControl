using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace CamControl
{
	public class Programm_tmp1
	{
		public static void Main(string[] args)
		{
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)

			.ConfigureAppConfiguration((hostingContext, config) =>
			{
				config.AddJsonFile("CameraConfig.json",
									optional: true,
									reloadOnChange: true); ;
			})
#if Debug
                .UseStartup<StartupDebug>()
#else
				.UseStartup<Startup_tmp>()
#endif
				.Build();
	}
}

