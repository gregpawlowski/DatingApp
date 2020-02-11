﻿using System;
using DatingApp.API.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DatingApp.API
{
  public class Program
    {
        public static void Main(string[] args)
        {
            // Web host builder
            var host = CreateWebHostBuilder(args).Build();
            // Create a scope that is only valid for when the application is starting.
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try 
                {
                    // Get access to the DataContext
                    var context = services.GetRequiredService<DataContext>();
                    // Apply any pending migrations, and crete the database if it doesn't exist
                    context.Database.Migrate();
                    // Seed the users 
                    Seed.SeedUsers(context);

                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occured during migration");
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
