using LibraryManagement.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    var roleManager = services
        .GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new string[] { "Admin", "Member" };
    for (int i = 0; i < roles.Length; i++)
    {
        var roleName = roles[i];
        var exists = await roleManager.RoleExistsAsync(roleName);
        if (!exists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var adminEmail = "admin@library.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin@1234!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();