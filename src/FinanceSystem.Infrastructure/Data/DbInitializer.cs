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
                // Verificar e criar roles padrão
                if (!await _context.Roles.AnyAsync())
                {
                    _logger.LogInformation("Creating default roles...");

                    var adminRole = new Role("Admin", "Administrator role with full access");
                    var userRole = new Role("User", "Standard user role with limited access");

                    await _context.Roles.AddRangeAsync(adminRole, userRole);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default roles created successfully");
                }

                // Verificar e criar usuário administrador padrão
                if (!await _context.Users.AnyAsync())
                {
                    _logger.LogInformation("Creating default admin user...");

                    // Obter a role Admin
                    var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                    if (adminRole == null)
                    {
                        _logger.LogError("Admin role not found. Cannot create admin user.");
                        return;
                    }

                    // Criar usuário admin
                    var adminUser = new User(
                        "admin",
                        "admin@example.com",
                        _authService.HashPassword("admin")
                    );

                    await _context.Users.AddAsync(adminUser);
                    await _context.SaveChangesAsync();

                    // Adicionar a role de Admin ao usuário
                    adminUser.AddRole(adminRole);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default admin user created successfully");
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