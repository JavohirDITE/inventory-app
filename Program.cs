using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Parse DATABASE_PUBLIC_URL from environment
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL") 
            ?? builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString = dbUrl ?? "";
if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    var db = uri.LocalPath.TrimStart('/');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={db};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Prefer;Trust Server Certificate=true;";
}

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        logger.LogInformation("Attempting to connect to the database...");
        bool canConnect = context.Database.CanConnect();
        
        if (canConnect)
        {
            logger.LogInformation("Successfully connected to the database.");
            
            logger.LogInformation("Checking for pending migrations...");
            var pendingMigrations = context.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                context.Database.Migrate();
                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No pending migrations. Database is up to date.");
            }
        }
        else
        {
            logger.LogWarning("Could not connect to the database. Migrations will not be applied.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while connecting or migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
