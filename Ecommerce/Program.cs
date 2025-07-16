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

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine("DB Connection configured: " + (!string.IsNullOrEmpty(connectionString) ? "Yes" : "No"));

        builder.Services.AddDbContext<MyContext>(options =>
            options.UseNpgsql(connectionString));

        // Cookie authentication configuration
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.LogoutPath = "/Account/Login";
            options.Cookie.Name = "Home"; // any name
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MyContext>();

            // Apply any pending migrations
            dbContext.Database.Migrate();

            // Seed data after migration
            SeedUsers(dbContext, app.Configuration);
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
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

    private static void SeedUsers(MyContext dbContext, IConfiguration configuration)
    {
        if (!dbContext.Users.Any())
        {
            var admin1Email = configuration["ADMIN1_EMAIL"];
            var admin1Username = configuration["ADMIN1_USERNAME"];
            var admin1Pass = configuration["ADMIN1_PASSWORD"];
            var admin2Email = configuration["ADMIN2_EMAIL"];
            var admin2Username = configuration["ADMIN2_USERNAME"];
            var admin2Pass = configuration["ADMIN2_PASSWORD"];

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
                    throw new InvalidOperationException($"Environment variable {name} is required but not set.");
                }
            }

            var users = new List<User>
            {
                new User
                {
                    email = admin1Email,
                    userName = admin1Username,
                    password = admin1Pass,
                    Role = "Admin",
                    Age = 22
                },
                new User
                {
                    email = admin2Email,
                    userName = admin2Username,
                    password = admin2Pass,
                    Role = "Admin",
                    Age = 18
                }
            };

            dbContext.Users.AddRange(users);
            dbContext.SaveChanges();
            Console.WriteLine("Admin users seeded successfully.");
        }
    }
}