using HMS.Data; // Namespace dla HMSContext

using HMS.Entities;
using HMS.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("HMSConnection")
    ?? throw new InvalidOperationException("Connection string 'HMSConnection' not found.");


// Konfiguracja DbContext dla HMSContext
builder.Services.AddDbContext<HMSContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging() // Tylko w œrodowiskach deweloperskich
           .LogTo(Console.WriteLine, LogLevel.Information));

builder.Services.AddDbContext<HMSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodanie Identity z User i Role
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Logowanie bez weryfikacji e-mail
    options.Password.RequireDigit = true;          // Wymagana cyfra w haœle
    options.Password.RequiredLength = 6;           // Minimalna d³ugoœæ has³a
    options.Password.RequireUppercase = false;     // Bez wymogu wielkich liter
    options.Password.RequireNonAlphanumeric = false; // Bez wymogu znaków specjalnych
})
.AddEntityFrameworkStores<HMSContext>()
.AddDefaultUI()
.AddDefaultTokenProviders(); // Tokeny do np. resetowania has³a

// Konfiguracja uwierzytelniania
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
});

// Rejestracja kontrolerów i widoków
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Dodanie obs³ugi b³êdów w czasie deweloperskim
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Rejestracja us³ug
builder.Services.AddScoped<AccommodationTypesService>();
builder.Services.AddScoped<AccommodationPackagesService>();
builder.Services.AddScoped<DishesService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AccommodationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<RoomService>();

builder.Logging.AddConsole();
builder.Logging.AddDebug();



var app = builder.Build();

// Konfiguracja potoku HTTP
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

// Middleware obs³ugi HTTP
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication(); // Obs³uga uwierzytelniania
app.UseAuthorization();  // Obs³uga autoryzacji

// Definicja routingu
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


// Uruchom aplikacjê
app.Run();
