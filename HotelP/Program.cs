using Hangfire;

using Hangfire.SqlServer;    // <-- to jest kluczowe

using HMS.Data; // Namespace dla HMSContext
using HMS.Entities;
using HMS.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1) ConnectionString i DbContext HMSContext ---
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

// --- 6) (Opcjonalnie) rejestracja AutoReleaseJob, jeœli jej konstruktor czegoœ wymaga ---
builder.Services.AddTransient<AutoReleaseJob>();

// --- 7) Hangfire: dodanie serwera i konfiguracji ---
builder.Services.AddHangfire(config =>
{
    // Mo¿emy u¿yæ tego samego connectionString
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

// --- 11) Œcie¿ki statyczne, HTTPS, routing, uwierzytelnianie ---
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// --- 12) Hangfire Dashboard (opcjonalnie) i serwer
//    UWAGA: app.UseHangfireServer() jest ju¿ wywo³ane przez AddHangfireServer();
//    wiêc tu wystarczy sam Dashboard, jeœli chcesz.
app.UseHangfireDashboard();

// --- 13) Definicja tras (mapowanie kontrolerów) ---
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


// --- 15) Uruchamianie aplikacji ---
app.Run();
