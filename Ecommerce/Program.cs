using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure logging first
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        Console.WriteLine("=== APPLICATION STARTUP ===");
        Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
        Console.WriteLine($"Application Name: {builder.Environment.ApplicationName}");

        // --- SERVICE CONFIGURATION ---

        // Get the connection string
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("ERROR: Database connection string 'DefaultConnection' was not found.");
            throw new InvalidOperationException("Database connection string 'DefaultConnection' was not found. Please ensure it is set correctly in Railway's environment variables.");
        }

        Console.WriteLine("✓ Database connection string found");
        // Log connection string without sensitive data
        var sanitizedConnectionString = SanitizeConnectionString(connectionString);
        Console.WriteLine($"Connection: {sanitizedConnectionString}");

        // Configure services
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

        // Add DbContext with retry policy for Railway
        builder.Services.AddDbContext<MyContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });

            // Enable sensitive data logging only in development
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Configure authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.Name = "EcommerceCookie";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.None
                    : CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;
            });

        var app = builder.Build();

        Console.WriteLine("✓ Services configured successfully");

        // Initialize database with better error handling
        try
        {
            InitializeDatabase(app);
            Console.WriteLine("✓ Database initialization completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");

            // Don't throw in production, let the app start without seeded data
            if (app.Environment.IsDevelopment())
            {
                throw;
            }
            else
            {
                Console.WriteLine("⚠️  Application will continue without database seeding in production");
            }
        }

        // Configure middleware pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        // Set up port for Railway
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        app.Urls.Add($"http://0.0.0.0:{port}");

        Console.WriteLine($"✓ Application configured to listen on port {port}");

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        Console.WriteLine("✓ Middleware pipeline configured");
        Console.WriteLine("=== STARTING APPLICATION ===");

        app.Run();
    }

    private static void InitializeDatabase(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var dbContext = services.GetRequiredService<MyContext>();

            Console.WriteLine("Testing database connection...");

            // Test connection first
            if (!dbContext.Database.CanConnect())
            {
                Console.WriteLine("❌ Cannot connect to database");
                throw new InvalidOperationException("Cannot connect to the database");
            }

            Console.WriteLine("✓ Database connection successful");

            Console.WriteLine("Applying database migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("✓ Database migrations applied successfully");

            // Seed admin users
            SeedAdminUsers(dbContext, services.GetRequiredService<IConfiguration>(), logger);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database initialization error: {ex.Message}");

            // Log more details for common Railway/PostgreSQL issues
            if (ex.Message.Contains("connection") || ex.Message.Contains("timeout"))
            {
                Console.WriteLine("This might be a Railway database connection issue. Please check:");
                Console.WriteLine("1. Database service is running");
                Console.WriteLine("2. Connection string is correct");
                Console.WriteLine("3. Network connectivity");
            }

            throw;
        }
    }

    private static void SeedAdminUsers(MyContext dbContext, IConfiguration configuration, ILogger logger)
    {
        Console.WriteLine("Checking if admin users need to be seeded...");

        if (dbContext.Users.Any())
        {
            Console.WriteLine("✓ Database already contains users. Skipping seed process.");
            return;
        }

        Console.WriteLine("No users found. Creating admin accounts...");

        var usersToSeed = new List<User>();

        // Admin 1
        var admin1Email = configuration["ADMIN1_EMAIL"];
        var admin1Username = configuration["ADMIN1_USERNAME"];
        var admin1Pass = configuration["ADMIN1_PASSWORD"];

        // Admin 2  
        var admin2Email = configuration["ADMIN2_EMAIL"];
        var admin2Username = configuration["ADMIN2_USERNAME"];
        var admin2Pass = configuration["ADMIN2_PASSWORD"];

        if (!string.IsNullOrEmpty(admin1Email) && !string.IsNullOrEmpty(admin1Username) && !string.IsNullOrEmpty(admin1Pass))
        {
            usersToSeed.Add(new User
            {
                email = admin1Email,
                userName = admin1Username,
                password = HashPassword(admin1Pass), // Hash the password!
                Role = "Admin",
                Age = 22
            });
            Console.WriteLine($"✓ Prepared Admin1: {admin1Username} ({admin1Email})");
        }
        else
        {
            Console.WriteLine("⚠️  Admin1 configuration incomplete");
        }

        if (!string.IsNullOrEmpty(admin2Email) && !string.IsNullOrEmpty(admin2Username) && !string.IsNullOrEmpty(admin2Pass))
        {
            usersToSeed.Add(new User
            {
                email = admin2Email,
                userName = admin2Username,
                password = HashPassword(admin2Pass), // Hash the password!
                Role = "Admin",
                Age = 18
            });
            Console.WriteLine($"✓ Prepared Admin2: {admin2Username} ({admin2Email})");
        }
        else
        {
            Console.WriteLine("⚠️  Admin2 configuration incomplete");
        }

        if (usersToSeed.Any())
        {
            try
            {
                dbContext.Users.AddRange(usersToSeed);
                dbContext.SaveChanges();
                Console.WriteLine($"✅ SUCCESS: Seeded {usersToSeed.Count} admin user(s) to the database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to seed users: {ex.Message}");
                throw;
            }
        }
        else
        {
            Console.WriteLine("⚠️  No admin users to seed. Check your environment variables.");
        }
    }

    // Helper method to hash passwords (basic implementation)
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "YourSaltHere"));
        return Convert.ToBase64String(hashedBytes);
    }

    // Helper method to sanitize connection string for logging
    private static string SanitizeConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "NULL";

        // Hide password from connection string for logging
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            @"Password=([^;]+)",
            "Password=***",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}