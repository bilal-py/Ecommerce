using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Collections;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

        // Try multiple ways to get the connection string
        var connectionString = GetConnectionString(builder.Configuration);

        Console.WriteLine($"Connection string found: {!string.IsNullOrEmpty(connectionString)}");
        Console.WriteLine($"Connection string length: {connectionString?.Length ?? 0}");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("ERROR: No connection string found!");
            Console.WriteLine("Available environment variables:");
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                if (env.Key.ToString().Contains("Connection") || env.Key.ToString().Contains("DATABASE"))
                {
                    Console.WriteLine($"  {env.Key}: {env.Value}");
                }
            }
            throw new InvalidOperationException("Database connection string is required but not found.");
        }

        builder.Services.AddDbContext<MyContext>(options =>
            options.UseNpgsql(connectionString));

        // Cookie authentication configuration
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Login";
            options.Cookie.Name = "Home";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ?
                CookieSecurePolicy.None : CookieSecurePolicy.Always;
        });

        var app = builder.Build();

        // Database migration and seeding
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MyContext>();
                Console.WriteLine("Attempting database migration...");

                // Apply any pending migrations
                dbContext.Database.Migrate();
                Console.WriteLine("Database migration completed successfully.");

                // Seed data after migration
                SeedUsers(dbContext, app.Configuration);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database migration failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        // Method 1: Railway environment variable with double underscore (should work)
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Found connection string via ConnectionStrings__DefaultConnection");
            return connectionString;
        }

        // Method 2: Standard configuration path (reads from appsettings + env vars)
        connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Found connection string via GetConnectionString method");
            return connectionString;
        }

        // Method 3: Direct configuration access
        connectionString = configuration["ConnectionStrings:DefaultConnection"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Found connection string via configuration section");
            return connectionString;
        }

        // Method 4: Railway's DATABASE_URL (if using Railway's built-in PostgreSQL)
        connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Found connection string via DATABASE_URL");
            return connectionString;
        }

        return null;
    }

    private static void SeedUsers(MyContext dbContext, IConfiguration configuration)
    {
        try
        {
            Console.WriteLine("Checking if users need to be seeded...");

            if (!dbContext.Users.Any())
            {
                Console.WriteLine("No users found, seeding admin users...");

                var admin1Email = GetConfigValue(configuration, "ADMIN1_EMAIL");
                var admin1Username = GetConfigValue(configuration, "ADMIN1_USERNAME");
                var admin1Pass = GetConfigValue(configuration, "ADMIN1_PASSWORD");
                var admin2Email = GetConfigValue(configuration, "ADMIN2_EMAIL");
                var admin2Username = GetConfigValue(configuration, "ADMIN2_USERNAME");
                var admin2Pass = GetConfigValue(configuration, "ADMIN2_PASSWORD");

                // Validate that all required environment variables are present
                var requiredVars = new[]
                {
                    (admin1Email, "ADMIN1_EMAIL"),
                    (admin1Username, "ADMIN1_USERNAME"),
                    (admin1Pass, "ADMIN1_PASSWORD"),
                    (admin2Email, "ADMIN2_EMAIL"),
                    (admin2Username, "ADMIN2_USERNAME"),
                    (admin2Pass, "ADMIN2_PASSWORD")
                };

                foreach (var (value, name) in requiredVars)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        Console.WriteLine($"WARNING: Environment variable {name} is not set.");
                        // Don't throw exception, just log warning for now
                    }
                }

                // Only seed if we have at least one complete admin user
                if (!string.IsNullOrEmpty(admin1Email) && !string.IsNullOrEmpty(admin1Username) && !string.IsNullOrEmpty(admin1Pass))
                {
                    var users = new List<User>
                    {
                        new User
                        {
                            email = admin1Email,
                            userName = admin1Username,
                            password = admin1Pass,
                            Role = "Admin",
                            Age = 22
                        }
                    };

                    // Add second admin if all details are available
                    if (!string.IsNullOrEmpty(admin2Email) && !string.IsNullOrEmpty(admin2Username) && !string.IsNullOrEmpty(admin2Pass))
                    {
                        users.Add(new User
                        {
                            email = admin2Email,
                            userName = admin2Username,
                            password = admin2Pass,
                            Role = "Admin",
                            Age = 18
                        });
                    }

                    dbContext.Users.AddRange(users);
                    dbContext.SaveChanges();
                    Console.WriteLine($"Successfully seeded {users.Count} admin user(s).");
                }
                else
                {
                    Console.WriteLine("WARNING: No admin users seeded due to missing environment variables.");
                }
            }
            else
            {
                Console.WriteLine("Users already exist, skipping seeding.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during user seeding: {ex.Message}");
            // Don't throw exception here, let the application continue
        }
    }

    private static string GetConfigValue(IConfiguration configuration, string key)
    {
        // Try configuration first, then environment variable
        return configuration[key] ?? Environment.GetEnvironmentVariable(key);
    }
}