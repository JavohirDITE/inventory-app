using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using InventoryApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Parse DATABASE_URL or DATABASE_PUBLIC_URL from environment
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL") 
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
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var authBuilder = builder.Services.AddAuthentication();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = googleClientId;
        googleOptions.ClientSecret = googleClientSecret;
        googleOptions.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
        googleOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    });
}

var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
{
    authBuilder.AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = facebookAppId;
        facebookOptions.AppSecret = facebookAppSecret;
    });
}

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

builder.Services.AddSignalR();

// Configure forwarded headers for reverse proxies (e.g. Railway)
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ru") };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

app.UseForwardedHeaders();
// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        
        // Sanitize bad seeded description that violates requirements in production DB
        await dbContext.Database.ExecuteSqlRawAsync(
            "UPDATE \"Inventories\" SET \"Description\" = 'Restricted write access. Viewable by everyone.', \"Title\" = 'Bob''s Sci-Fi Collection' WHERE \"Description\" LIKE '%Private items not visible to guests%'");
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

    // Seed Admin Role and User
    try 
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        if (!string.IsNullOrEmpty(adminEmail))
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation($"Granted Admin role to user with email: {adminEmail}");
            }
        }

        // Seed test data
        var appContext = services.GetRequiredService<ApplicationDbContext>();
        var userMgr = services.GetRequiredService<UserManager<IdentityUser>>();
        await DbSeeder.SeedDataAsync(appContext, userMgr);
    } 
    catch (Exception ex) 
    {
        logger.LogError(ex, "Error seeding roles, admin, or test data.");
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
app.UseRequestLocalization();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Unspecified,
    Secure = CookieSecurePolicy.Always
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();
app.MapHub<InventoryApp.Hubs.DiscussionHub>("/discussionHub");

app.Run();
