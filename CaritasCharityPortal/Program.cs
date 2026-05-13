using CaritasCharityPortal.Controllers;
using CaritasCharityPortal.Services;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.LogoutPath = "/Account/Logout";
    });

builder.Services.AddSingleton<IDonationService, FirebaseDonationService>();
builder.Services.AddSingleton<IVolunteerService, FirebaseVolunteerService>();
builder.Services.AddSingleton<IReceiptService, FirebaseReceiptService>();
builder.Services.AddSingleton<FirebaseAuthService>();
builder.Services.AddSingleton<IAssistanceService, FirebaseAssistanceService>();
builder.Services.AddSingleton<ICommunityService, FirebaseCommunityService>();
builder.Services.AddSingleton<IReportService, FirebaseReportService>();
builder.Services.AddSingleton<IInventoryService, FirebaseInventoryService>();
builder.Services.AddSingleton<IDashboardService, FirebaseDashboardService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ========== SEED ADMIN USER ==========
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var databaseUrl = config["Firebase:DatabaseUrl"];
    var client = new FirebaseClient(databaseUrl);
    var authService = new FirebaseAuthService();

    try
    {
        var adminUser = authService.GetUserByEmailAsync("admin@caritas.com").GetAwaiter().GetResult();
        if (adminUser == null)
        {
            adminUser = authService.CreateUserAsync("admin@caritas.com", "Admin@123", "System Admin").GetAwaiter().GetResult();
        }

        var userNode = client.Child("users").Child(adminUser.Uid).OnceSingleAsync<UserDb>().GetAwaiter().GetResult();
        if (userNode == null)
        {
            var adminDb = new UserDb
            {
                FullName = "System Administrator",
                Email = "admin@caritas.com",
                Phone = "+27777777777",
                Location = "Head Office",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };
            client.Child("users").Child(adminUser.Uid).PutAsync(adminDb).GetAwaiter().GetResult();
            Console.WriteLine("✅ Admin user created.");
        }
        else if (userNode.Role != "Admin")
        {
            userNode.Role = "Admin";
            client.Child("users").Child(adminUser.Uid).PutAsync(userNode).GetAwaiter().GetResult();
            Console.WriteLine("✅ Admin role updated.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Admin seed error: {ex.Message}");
    }
}


app.Run();