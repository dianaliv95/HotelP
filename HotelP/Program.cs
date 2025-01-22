using Hangfire;

using Hangfire.SqlServer;    // <-- to jest kluczowe

using HMS.Data; // Namespace dla HMSContext
using HMS.Entities;
using HMS.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// --- 1) ConnectionString i DbContext HMSContext ---
var connectionString = builder.Configuration.GetConnectionString("HMSConnection")
    ?? throw new InvalidOperationException("Connection string 'HMSConnection' not found.");

builder.Services.AddDbContext<HMSContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging() // Tylko w �rodowiskach dev
           .LogTo(Console.WriteLine, LogLevel.Information));

// --- 2) Identity ---
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Logowanie bez weryfikacji e-mail
    options.Password.RequireDigit = true;           // Wymagana cyfra w ha�le
    options.Password.RequiredLength = 6;            // Minimalna d�ugo�� has�a
    options.Password.RequireUppercase = false;      // Bez wymogu wielkich liter
    options.Password.RequireNonAlphanumeric = false;// Bez wymogu znak�w specjalnych
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

// --- 5) Rejestracja us�ug aplikacji (DI) ---
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

// --- 6) (Opcjonalnie) rejestracja AutoReleaseJob, je�li jej konstruktor czego� wymaga ---
builder.Services.AddTransient<AutoReleaseJob>();

// --- 7) Hangfire: dodanie serwera i konfiguracji ---
builder.Services.AddHangfire(config =>
{
    // Mo�emy u�y� tego samego connectionString
    config.UseSqlServerStorage(connectionString);
});
// Dodaj serwer Hangfire
builder.Services.AddHangfireServer();

// --- 8) Logging do konsoli/debug ---
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// --- 9) Konfiguracja ciasteczek Identity ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
var cultureInfo = new CultureInfo("en-US");
// i w UseRequestLocalization
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Mo�esz skonfigurowa� RequestLocalization:

var app = builder.Build();


// --- 10) Konfiguracja trybu dev vs. prod ---
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

// --- 11) �cie�ki statyczne, HTTPS, routing, uwierzytelnianie ---
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// --- 12) Hangfire Dashboard (opcjonalnie) i serwer
//    UWAGA: app.UseHangfireServer() jest ju� wywo�ane przez AddHangfireServer();
//    wi�c tu wystarczy sam Dashboard, je�li chcesz.
app.UseHangfireDashboard();


// Dalej routing, kontrolery itp.
app.MapControllers();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = new[] { cultureInfo },
    SupportedUICultures = new[] { cultureInfo }
});
// --- 13) Definicja tras (mapowanie kontroler�w) ---
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

// --- 14) Rejestrowanie zadania cyklicznego Hangfire (po zarejestrowaniu serwera) ---
RecurringJob.AddOrUpdate<AutoReleaseJob>(
    "auto-release-rooms",
    job => job.RunAsync(),
    "5 0 * * *"  // codziennie o 00:05
);

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1) Tworzymy (je�li nie istnieje) rol� "GlobalAdmin"
        string adminRoleName = "Admin";
        var adminRoleExists = await roleManager.RoleExistsAsync(adminRoleName);
        if (!adminRoleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        // 2) Sprawdzamy, czy jest ju� user "admin"
        string adminUsername = "admin@hotel.com";
        string adminPassword = "adminadmin1";

        // Uwaga: je�eli chcesz u�y� Email = "admin@hotel.com" i Username = "admin", to:
        //  var adminUser = await userManager.FindByNameAsync(adminUsername);
        // ewentualnie rozejrzyj si�, czy w UserName i Email chcesz to samo
        var adminUser = await userManager.FindByNameAsync(adminUsername);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminUsername,
                Email = "admin@hotel.com", // albo "admin", je�li mail nie jest walidowany
                EmailConfirmed = true      // opcjonalnie oznacz od razu mail jako potwierdzony
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                // Przypisz go do roli GlobalAdmin
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
            else
            {
                // Obs�uga b��d�w tworzenia usera
                // (np. log do pliku, do Debug)
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"B��d tworzenia admina: {error.Description}");
                }
            }
        }
        else
        {
            // Mamy usera "admin" � sprawd�, czy ma przypisan� rol�:
            var rolesForAdmin = await userManager.GetRolesAsync(adminUser);
            if (!rolesForAdmin.Contains(adminRoleName))
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }
    }
    catch (Exception ex)
    {
        // Logowanie b��du
        Console.WriteLine($"[SEED ADMIN ERROR]: {ex.Message}");
    }
}

// --- 15) Uruchamianie aplikacji ---
app.Run();
