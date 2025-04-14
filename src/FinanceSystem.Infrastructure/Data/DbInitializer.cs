using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceSystem.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(
            ApplicationDbContext context,
            IAuthService authService,
            ILogger<DbInitializer> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        public async Task Initialize()
        {
            try
            {
                _logger.LogInformation("Applying database migrations...");
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying migrations");
                throw;
            }
        }

        public async Task SeedData()
        {
            try
            {
                if (!await _context.Permissions.AnyAsync())
                {
                    _logger.LogInformation("Creating default permissions...");

                    var viewUsersPermission = new Permission("View Users", "users.view", "Permission to view users");
                    var createUsersPermission = new Permission("Create Users", "users.create", "Permission to create users");
                    var updateUsersPermission = new Permission("Edit Users", "users.edit", "Permission to edit users");
                    var deleteUsersPermission = new Permission("Delete Users", "users.delete", "Permission to delete users");

                    var viewRolesPermission = new Permission("View Roles", "roles.view", "Permission to view roles");
                    var createRolesPermission = new Permission("Create Roles", "roles.create", "Permission to create roles");
                    var updateRolesPermission = new Permission("Edit Roles", "roles.edit", "Permission to edit roles");
                    var deleteRolesPermission = new Permission("Delete Roles", "roles.delete", "Permission to delete roles");

                    var managePermissionsPermission = new Permission("Manage Permissions", "permissions.manage", "Permission to manage permissions");

                    await _context.Permissions.AddRangeAsync(
                        viewUsersPermission, createUsersPermission, updateUsersPermission, deleteUsersPermission,
                        viewRolesPermission, createRolesPermission, updateRolesPermission, deleteRolesPermission,
                        managePermissionsPermission);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default permissions created successfully");
                }

                if (!await _context.Roles.AnyAsync())
                {
                    _logger.LogInformation("Creating default roles...");

                    var adminRole = new Role("Admin", "Administrator role with full access");
                    var moderatorRole = new Role("Moderator", "Moderator role with limited access");
                    var userRole = new Role("User", "Standard user role with minimal access");

                    await _context.Roles.AddRangeAsync(adminRole, moderatorRole, userRole);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default roles created successfully");

                    _logger.LogInformation("Assigning permissions to roles...");

                    var allPermissions = await _context.Permissions.ToListAsync();

                    foreach (var permission in allPermissions)
                    {
                        adminRole.AddPermission(permission);
                    }

                    var moderatorPermissions = allPermissions
                        .Where(p => p.SystemName == "users.view" || 
                                    p.SystemName == "users.create" || 
                                    p.SystemName == "users.edit" || 
                                    p.SystemName == "roles.view").ToList();

                    foreach (var permission in moderatorPermissions)
                    {
                        moderatorRole.AddPermission(permission);
                    }

                    var userPermissions = allPermissions
                        .Where(p => p.SystemName == "users.view").ToList();

                    foreach (var permission in userPermissions)
                    {
                        userRole.AddPermission(permission);
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Permissions assigned to roles successfully");
                }

                if (!await _context.Users.AnyAsync())
                {
                    _logger.LogInformation("Creating default admin user...");

                    var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                    if (adminRole == null)
                    {
                        _logger.LogError("Admin role not found. Cannot create admin user.");
                        return;
                    }

                    var adminUser = new User("admin", "admin@example.com", _authService.HashPassword("admin"));

                    await _context.Users.AddAsync(adminUser);
                    await _context.SaveChangesAsync();

                    adminUser.AddRole(adminRole);
                    await _context.SaveChangesAsync();

                    var moderatorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Moderator");
                    if (moderatorRole != null)
                    {
                        var moderatorUser = new User(
                            "moderator",
                            "moderator@example.com",
                            _authService.HashPassword("moderator")
                        );

                        await _context.Users.AddAsync(moderatorUser);
                        await _context.SaveChangesAsync();

                        moderatorUser.AddRole(moderatorRole);
                        await _context.SaveChangesAsync();
                    }

                    var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                    if (userRole != null)
                    {
                        var regularUser = new User(
                            "user",
                            "user@example.com",
                            _authService.HashPassword("user")
                        );

                        await _context.Users.AddAsync(regularUser);
                        await _context.SaveChangesAsync();

                        regularUser.AddRole(userRole);
                        await _context.SaveChangesAsync();
                    }

                    _logger.LogInformation("Default users created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding data");
                throw;
            }
        }
    }
}