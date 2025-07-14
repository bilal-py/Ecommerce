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

        //builder.Services.AddDbContext<MyContext>();

        //builder.Services.AddDbContext<MyContext>(options =>
        //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        //    )
        //);
        var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
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
            SeedUsers(dbContext);
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

    private static void SeedUsers(MyContext dbContext)
    {
        if (!dbContext.Users.Any())
        {
            var admin1Email = Environment.GetEnvironmentVariable("ADMIN1_EMAIL");
            var admin1Username = Environment.GetEnvironmentVariable("ADMIN1_USERNAME");
            var admin1Pass = Environment.GetEnvironmentVariable("ADMIN1_PASSWORD");
            var admin2Email = Environment.GetEnvironmentVariable("ADMIN2_EMAIL");
            var admin2Username = Environment.GetEnvironmentVariable("ADMIN2_USERNAME");
            var admin2Pass = Environment.GetEnvironmentVariable("ADMIN2_PASSWORD");

            var users = new List<User>
            {
            new User
            {
                email = admin1Email,
                userName = admin1Username,
                password = admin1Pass,
                Role = "Admin",
                Age= 22
            },
            new User
            {
                email = admin2Email,
                userName = admin2Username,
                password = admin2Pass,
                Role = "Admin",
                Age= 18
            }
        };


            dbContext.Users.AddRange(users);
            dbContext.SaveChanges();
        }
    }
}
