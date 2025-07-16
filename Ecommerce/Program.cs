using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

        // Direct environment variable access - this should work based on your debug output
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        Console.WriteLine($"Direct env var result: '{connectionString}'");
        Console.WriteLine($"Connection string length: {connectionString?.Length ?? 0}");
        Console.WriteLine($"Connection string is null or empty: {string.IsNullOrEmpty(connectionString)}");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("ERROR: Connection string is still null or empty!");

            // Let's try a different approach - check if there are any invisible characters
            var rawEnvVar = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            if (rawEnvVar != null)
            {
                Console.WriteLine($"Raw env var exists but is: '{rawEnvVar}'");
                Console.WriteLine($"Raw env var length: {rawEnvVar.Length}");
                Console.WriteLine($"Raw env var trimmed: '{rawEnvVar.Trim()}'");
                connectionString = rawEnvVar.Trim();
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string is required but not found or is empty.");
            }
        }

        Console.WriteLine("✓ Using connection string successfully");

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
                SeedUsers(dbContext);
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

    private static void SeedUsers(MyContext dbContext)
    {
        try
        {
            Console.WriteLine("Checking if users need to be seeded...");

            if (!dbContext.Users.Any())
            {
                Console.WriteLine("No users found, seeding admin users...");

                var admin1Email = Environment.GetEnvironmentVariable("ADMIN1_EMAIL");
                var admin1Username = Environment.GetEnvironmentVariable("ADMIN1_USERNAME");
                var admin1Pass = Environment.GetEnvironmentVariable("ADMIN1_PASSWORD");
                var admin2Email = Environment.GetEnvironmentVariable("ADMIN2_EMAIL");
                var admin2Username = Environment.GetEnvironmentVariable("ADMIN2_USERNAME");
                var admin2Pass = Environment.GetEnvironmentVariable("ADMIN2_PASSWORD");

                Console.WriteLine($"Admin1 Email: {admin1Email}");
                Console.WriteLine($"Admin1 Username: {admin1Username}");
                Console.WriteLine($"Admin1 Password: {(!string.IsNullOrEmpty(admin1Pass) ? "***SET***" : "NOT SET")}");

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
}