using Hangfire;

using Hangfire.SqlServer;    

using HMS.Data; // Namespace dla HMSContext
using HMS.Entities;
using HMS.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("HMSConnection")
    ?? throw new InvalidOperationException("Connection string 'HMSConnection' not found.");

builder.Services.AddDbContext<HMSContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging() // Tylko w œrodowiskach dev
           .LogTo(Console.WriteLine, LogLevel.Information));

// --- 2) Identity ---
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Logowanie bez weryfikacji e-mail
    options.Password.RequireDigit = true;           // Wymagana cyfra w haœle
    options.Password.RequiredLength = 6;            // Minimalna d³ugoœæ has³a
    options.Password.RequireUppercase = false;      // Bez wymogu wielkich liter
    options.Password.RequireNonAlphanumeric = false;// Bez wymogu znaków specjalnych
})
.AddEntityFrameworkStores<HMSContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// --- 3) Uwierzytelnianie + autoryzacja ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
});

// --- 4) MVC + Razor + DeveloperPageExceptionFilter ---
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- 5) Rejestracja us³ug aplikacji (DI) ---
builder.Services.AddScoped<AccommodationTypesService>();
builder.Services.AddScoped<AccommodationPackagesService>();
builder.Services.AddScoped<DishesService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AccommodationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<GroupBookingService>();

builder.Services.AddTransient<AutoReleaseJob>();

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(connectionString);
});
builder.Services.AddHangfireServer();

builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.UseHangfireDashboard();


app.MapControllers();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = new[] { cultureInfo },
    SupportedUICultures = new[] { cultureInfo }
});
app.MapControllerRoute(
    name: "Accomodations",
    pattern: "Accomodations/{action=Index}/{id?}",
    defaults: new { controller = "Accomodations", action = "Index", area = "" }
);

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

RecurringJob.AddOrUpdate<AutoReleaseJob>(
    "auto-release-rooms",
    job => job.RunAsync(),
    "5 0 * * *" 
);

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string adminRoleName = "Admin";
        var adminRoleExists = await roleManager.RoleExistsAsync(adminRoleName);
        if (!adminRoleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        string adminUsername = "admin@hotel.com";
        string adminPassword = "adminadmin1";

        
        var adminUser = await userManager.FindByNameAsync(adminUsername);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminUsername,
                Email = "admin@hotel.com", 
                EmailConfirmed = true      
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
            else
            {
                
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"B³¹d tworzenia admina: {error.Description}");
                }
            }
        }
        else
        {
            var rolesForAdmin = await userManager.GetRolesAsync(adminUser);
            if (!rolesForAdmin.Contains(adminRoleName))
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SEED ADMIN ERROR]: {ex.Message}");
    }
}

app.Run();
