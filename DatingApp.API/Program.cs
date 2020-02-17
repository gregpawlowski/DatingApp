using System;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DatingApp.API
{
  public class Program
    {
        public static void Main(string[] args)
        {
            // Web host builder
            var host = CreateHostBuilder(args).Build();
            // Create a scope that is only valid for when the application is starting.
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try 
                {
                    // Get access to the DataContext
                    var context = services.GetRequiredService<DataContext>();
                    // Get User Manager
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    // Get The Role Manager, used to pass into the SeedUsers method.
                    var roleManager = services.GetRequiredService<RoleManager<Role>>();
                    // Apply any pending migrations, and crete the database if it doesn't exist
                    context.Database.Migrate();
                    // Seed the users 
                    Seed.SeedUsers(userManager, roleManager);

                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occured during migration");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
            });
    }
}
