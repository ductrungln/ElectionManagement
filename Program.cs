using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ElectionManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace ElectionManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // ensure database is created at startup so first request won't fail
            try
            {
                using (var scope = host.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ElectionDbContext>();
                    context.Database.EnsureCreated();
                    Console.WriteLine("[INFO] Database ensured/created at application startup");

                    // Columns are now handled by migration - no additional setup needed
                    Console.WriteLine("[INFO] Database schema is ready");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Failed to ensure database at startup: " + ex);
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
