using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DigitalWizardry.Jogtracks
{
	public class Startup
	{
		public IConfigurationRoot Configuration { get; }
		
		public Startup(IHostingEnvironment env)
		{
			// This line of code can be used to view the directory structure in Docker.
			// ListDirectory(Directory.GetParent(env.WebRootPath).FullName);

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("secrets.json", optional: false);
			Configuration = builder.Build();
		}
				
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddEntityFrameworkSqlServer().AddDbContext<MainContext>(options => options.UseSqlServer(Configuration["Secrets:ConnectionString"]));
			services.AddEntityFrameworkSqlServer().AddDbContext<ServiceLogContext>(options => options.UseSqlServer(Configuration["Secrets:ConnectionString"]));

			services.AddMvc();
			services.AddScoped<IServiceLogRepository, ServiceLogRepository>();
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IJogRepository, JogRepository>();

			services.Configure<Secrets>(secrets =>
			{
				secrets.BasicAuthUsername = Configuration["Secrets:BasicAuthUsername"];
				secrets.BasicAuthPassword = Configuration["Secrets:BasicAuthPassword"];
				secrets.ConnectionString = Configuration["Secrets:ConnectionString"];
			});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole();
			loggerFactory.AddDebug();
			
			app.UseMvc();
			app.UseStaticFiles();

			app.UseMvc
			(
				routes =>
				{
					routes.MapRoute
					(
						name: "default",
						template: "{controller=Index}/{action=Index}/{id?}"
					);
				}
			);
		}

		static void ListDirectory(string dir)
		{
			try
			{
				foreach (string f in Directory.GetFiles(dir))
					Console.WriteLine(f);
				foreach (string d in Directory.GetDirectories(dir))
				{
					Console.WriteLine(d);
					ListDirectory(d);
				}
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
