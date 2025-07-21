using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {

        // TEMPORARY DEBUGGING: Add this block to see all variables
        Console.WriteLine("---- DUMPING ALL ENVIRONMENT VARIABLES ----");
        foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
        {
            Console.WriteLine($"{env.Key} = {env.Value}");
        }
        Console.WriteLine("------------------------------------------");

        var builder = WebApplication.CreateBuilder(args);

        // This single line sets up configuration to read from:
        // 1. appsettings.json
        // 2. appsettings.Production.json (since ASPNETCORE_ENVIRONMENT is 'Production' in your Dockerfile)
        // 3. Environment Variables (which will override the files)

        // --- SERVICE CONFIGURATION ---

        // 1. Get the connection string directly from the configuration system.
        // It will automatically find and use the "ConnectionStrings__DefaultConnection"
        // variable from your Railway settings.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // 2. Add a "fail-fast" check. If the connection string isn't found, the app will
        // stop immediately with a clear error message.
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("CRITICAL ERROR: Database connection string 'DefaultConnection' was not found. Please ensure it is set correctly in Railway's environment variables.");
        }

        // 3. Configure your services.
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

        // 4. Add the DbContext using the connection string we just retrieved.
        builder.Services.AddDbContext<MyContext>(options =>
            options.UseNpgsql(connectionString));

        // 5. Configure cookie authentication.
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.Cookie.Name = "YourAppCookieName"; // It's good practice to give this a unique name
            options.Cookie.HttpOnly = true;
            // This logic is correct: secure cookie in production, non-secure in development
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.None
                : CookieSecurePolicy.Always;
        });

        // --- APPLICATION AND PIPELINE CONFIGURATION ---

        var app = builder.Build();

        // This is a good place to apply migrations and seed data.
        InitializeDatabase(app);

        // Configure the HTTP request pipeline (middleware).
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication(); // Important: comes before UseAuthorization
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }

    // Helper method to keep the Main method clean. This handles database setup.
    private static void InitializeDatabase(IApplicationBuilder app)
    {
        // 'CreateScope' is the correct way to get services like DbContext during app startup.
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var dbContext = services.GetRequiredService<MyContext>();
                Console.WriteLine("Attempting to apply database migrations...");
                // This applies any pending migrations to the database.
                dbContext.Database.Migrate();
                Console.WriteLine("Database migrations applied successfully.");

                // Now, seed the initial user data.
                SeedAdminUsers(dbContext, services.GetRequiredService<IConfiguration>());
            }
            catch (Exception ex)
            {
                // Using a proper logger is better, but for now, this shows the error clearly.
                Console.WriteLine($"FATAL: An error occurred during database initialization: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                // Re-throwing the exception will stop the application from starting in a broken state.
                throw;
            }
        }
    }

    // Simplified seeding method.
    private static void SeedAdminUsers(MyContext dbContext, IConfiguration configuration)
    {
        Console.WriteLine("Checking if admin users need to be seeded...");

        if (dbContext.Users.Any())
        {
            Console.WriteLine("Database already contains users. Skipping seed process.");
            return;
        }

        Console.WriteLine("No users found. Attempting to seed admin accounts...");

        // Read admin details DIRECTLY from configuration.
        // It automatically checks environment variables.
        var admin1Email = configuration["ADMIN1_EMAIL"];
        var admin1Username = configuration["ADMIN1_USERNAME"];
        var admin1Pass = configuration["ADMIN1_PASSWORD"];

        var admin2Email = configuration["ADMIN2_EMAIL"];
        var admin2Username = configuration["ADMIN2_USERNAME"];
        var admin2Pass = configuration["ADMIN2_PASSWORD"];

        var usersToSeed = new List<User>();

        if (!string.IsNullOrEmpty(admin1Email) && !string.IsNullOrEmpty(admin1Username) && !string.IsNullOrEmpty(admin1Pass))
        {
            usersToSeed.Add(new User
            {
                email = admin1Email,
                userName = admin1Username,
                password = admin1Pass, // Note: You should be HASHING passwords, not storing them in plain text!
                Role = "Admin",
                Age = 22
            });
            Console.WriteLine($"Found configuration for Admin1: {admin1Username}");
        }

        if (!string.IsNullOrEmpty(admin2Email) && !string.IsNullOrEmpty(admin2Username) && !string.IsNullOrEmpty(admin2Pass))
        {
            usersToSeed.Add(new User
            {
                email = admin2Email,
                userName = admin2Username,
                password = admin2Pass, // Note: You should be HASHING passwords!
                Role = "Admin",
                Age = 18
            });
            Console.WriteLine($"Found configuration for Admin2: {admin2Username}");
        }

        if (usersToSeed.Any())
        {
            dbContext.Users.AddRange(usersToSeed);
            dbContext.SaveChanges();
            Console.WriteLine($"SUCCESS: Seeded {usersToSeed.Count} admin user(s) to the database.");
        }
        else
        {
            Console.WriteLine("WARNING: Could not seed any admin users. Check your ADMIN# environment variables in Railway.");
        }
    }
}