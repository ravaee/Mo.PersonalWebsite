using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Mo.PersonalWebsite.Infrastructure.Data;
using Mo.PersonalWebsite.Features.Auth.AdminLogin.Models;
using Mo.PersonalWebsite.Features.Auth.AdminLogin.Services;
using Mo.PersonalWebsite.Features.Articles.Services;
using Mo.PersonalWebsite.Features.Categories.Services;
using Mo.PersonalWebsite.Features.Images.Services;
using Mo.PersonalWebsite.Features.Pages.Services;
using Mo.PersonalWebsite.Features.TestData.Services;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Add API controller support

// Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.LogoutPath = "/Admin/Logout";
        options.AccessDeniedPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));
});

// Configuration options
builder.Services.Configure<AdminUserOptions>(
    builder.Configuration.GetSection(AdminUserOptions.SectionName));

// Application services
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<ITestDataService, TestDataService>();

// FluentValidation
// builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static files with cache control headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLowerInvariant();
        
        // Set cache control based on file type
        if (path.EndsWith(".css") || path.EndsWith(".js"))
        {
            // CSS and JS files: Cache for 1 hour but allow revalidation
            ctx.Context.Response.Headers.CacheControl = "public, max-age=3600, must-revalidate";
            ctx.Context.Response.Headers.ETag = $"\"{ctx.File.LastModified:yyyyMMddHHmmss}\"";
        }
        else if (path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".png") || 
                 path.EndsWith(".gif") || path.EndsWith(".svg") || path.EndsWith(".ico"))
        {
            // Images: Cache for 24 hours
            ctx.Context.Response.Headers.CacheControl = "public, max-age=86400";
        }
        else
        {
            // Other files: Short cache with revalidation
            ctx.Context.Response.Headers.CacheControl = "public, max-age=1800, must-revalidate";
        }
    }
});

// Ensure uploads directory and configure file serving
var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

var articlesUploadsPath = Path.Combine(uploadsPath, "articles");
if (!Directory.Exists(articlesUploadsPath))
{
    Directory.CreateDirectory(articlesUploadsPath);
}

app.UseRouting();

// Add middleware to prevent caching of HTML pages
app.Use(async (context, next) =>
{
    await next();
    
    // Add no-cache headers for HTML responses to ensure fresh content
    if (context.Response.ContentType?.Contains("text/html") == true)
    {
        context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); // Add API controller routing

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        
        // Apply any pending migrations
        context.Database.Migrate();
        
        logger.LogInformation("Database migrations applied successfully.");
        
        // Seed default admin user if it doesn't exist
        SeedDefaultAdminUser(scope.ServiceProvider, logger);
        
        // Seed default categories
        await SeedDefaultCategories(scope.ServiceProvider, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw; // Re-throw to prevent app from starting with database issues
    }
}

void SeedDefaultAdminUser(IServiceProvider serviceProvider, ILogger logger)
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    
    var adminUsername = configuration["AdminUser:Username"];
    var adminPassword = configuration["AdminUser:Password"];
    
    if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminPassword))
    {
        logger.LogWarning("Admin user credentials not configured. Skipping admin user seeding.");
        return;
    }
    
    logger.LogInformation("Admin user configuration found: {Username}", adminUsername);
    logger.LogInformation("Database migrations and admin user setup completed successfully.");
}

async Task SeedDefaultCategories(IServiceProvider serviceProvider, ILogger logger)
{
    var categoryService = serviceProvider.GetRequiredService<ICategoryService>();
    
    try
    {
        // Ensure "General" category exists
        var generalCategory = await categoryService.GetOrCreateAsync("General");
        logger.LogInformation("Default 'General' category ensured with ID: {CategoryId}", generalCategory.Id);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding default categories");
    }
}

app.Run();
