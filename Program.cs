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

                    // add Level columns if they don't exist (SQLite doesn't support IF NOT EXISTS in ALTER TABLE)
                    try
                    {
                        var conn = context.Database.GetDbConnection();
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            // helper function to check column
                            Func<string, string, bool> hasCol = (table, col) =>
                            {
                                cmd.CommandText = $"PRAGMA table_info('{table}')";
                                using var reader = cmd.ExecuteReader();
                                while (reader.Read())
                                {
                                    if (reader.GetString(1).Equals(col, StringComparison.OrdinalIgnoreCase))
                                        return true;
                                }
                                return false;
                            };

                            if (!hasCol("ElectionResults", "Level"))
                            {
                                cmd.CommandText = "ALTER TABLE ElectionResults ADD COLUMN Level TEXT";
                                cmd.ExecuteNonQuery();
                                Console.WriteLine("[INFO] Added Level column to ElectionResults");
                            }
                            if (!hasCol("ElectionProgresses", "Level"))
                            {
                                cmd.CommandText = "ALTER TABLE ElectionProgresses ADD COLUMN Level TEXT";
                                cmd.ExecuteNonQuery();
                                Console.WriteLine("[INFO] Added Level column to ElectionProgresses");
                            }
                            if (!hasCol("ImportLogs", "Level"))
                            {
                                cmd.CommandText = "ALTER TABLE ImportLogs ADD COLUMN Level TEXT";
                                cmd.ExecuteNonQuery();
                                Console.WriteLine("[INFO] Added Level column to ImportLogs");
                            }
                            if (!hasCol("BallotVerifications", "Level"))
                            {
                                cmd.CommandText = "ALTER TABLE BallotVerifications ADD COLUMN Level TEXT";
                                cmd.ExecuteNonQuery();
                                Console.WriteLine("[INFO] Added Level column to BallotVerifications");
                            }
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[ERROR] Checking/adding Level columns failed: " + ex);
                    }
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
