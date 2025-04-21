using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Enums;
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

                    var viewPaymentsPermission = new Permission("View Payments", "payments.view", "Permission to view payments");
                    var createPaymentsPermission = new Permission("Create Payments", "payments.create", "Permission to create payments");
                    var updatePaymentsPermission = new Permission("Edit Payments", "payments.edit", "Permission to edit payments");
                    var deletePaymentsPermission = new Permission("Delete Payments", "payments.delete", "Permission to delete payments");

                    var viewPaymentTypesPermission = new Permission("View Payment Types", "paymenttypes.view", "Permission to view payment types");
                    var createPaymentTypesPermission = new Permission("Create Payment Types", "paymenttypes.create", "Permission to create payment types");
                    var updatePaymentTypesPermission = new Permission("Edit Payment Types", "paymenttypes.edit", "Permission to edit payment types");
                    var deletePaymentTypesPermission = new Permission("Delete Payment Types", "paymenttypes.delete", "Permission to delete payment types");

                    var viewPaymentMethodsPermission = new Permission("View Payment Methods", "paymentmethods.view", "Permission to view payment methods");
                    var createPaymentMethodsPermission = new Permission("Create Payment Methods", "paymentmethods.create", "Permission to create payment methods");
                    var updatePaymentMethodsPermission = new Permission("Edit Payment Methods", "paymentmethods.edit", "Permission to edit payment methods");
                    var deletePaymentMethodsPermission = new Permission("Delete Payment Methods", "paymentmethods.delete", "Permission to delete payment methods");

                    var viewCreditCardsPermission = new Permission("View Credit Cards", "creditcards.view", "Permission to view credit cards");
                    var createCreditCardsPermission = new Permission("Create Credit Cards", "creditcards.create", "Permission to create credit cards");
                    var updateCreditCardsPermission = new Permission("Edit Credit Cards", "creditcards.edit", "Permission to edit credit cards");
                    var deleteCreditCardsPermission = new Permission("Delete Credit Cards", "creditcards.delete", "Permission to delete credit cards");

                    var viewIncomesPermission = new Permission("View Incomes", "incomes.view", "Permission to view incomes");
                    var createIncomesPermission = new Permission("Create Incomes", "incomes.create", "Permission to create incomes");
                    var updateIncomesPermission = new Permission("Edit Incomes", "incomes.edit", "Permission to edit incomes");
                    var deleteIncomesPermission = new Permission("Delete Incomes", "incomes.delete", "Permission to delete incomes");

                    await _context.Permissions.AddAsync(managePermissionsPermission);
                    await _context.SaveChangesAsync();
                    await _context.Permissions.AddRangeAsync(viewUsersPermission, createUsersPermission, updateUsersPermission, deleteUsersPermission);
                    await _context.SaveChangesAsync();
                    await _context.Permissions.AddRangeAsync(viewRolesPermission, createRolesPermission, updateRolesPermission, deleteRolesPermission);
                    await _context.SaveChangesAsync();
                    await _context.Permissions.AddRangeAsync(viewPaymentsPermission, createPaymentsPermission, updatePaymentsPermission, deletePaymentsPermission);
                    await _context.SaveChangesAsync();
                    await _context.Permissions.AddRangeAsync(viewPaymentTypesPermission, createPaymentTypesPermission, updatePaymentTypesPermission, deletePaymentTypesPermission);
                    await _context.SaveChangesAsync();
                    await _context.Permissions.AddRangeAsync(viewPaymentMethodsPermission, createPaymentMethodsPermission, updatePaymentMethodsPermission, deletePaymentMethodsPermission);
                    await _context.SaveChangesAsync();
                    await _context.Permissions.AddRangeAsync(viewCreditCardsPermission, createCreditCardsPermission, updateCreditCardsPermission, deleteCreditCardsPermission);
                    await _context.SaveChangesAsync();
                    await _context.Permissions.AddRangeAsync(viewIncomesPermission, createIncomesPermission, updateIncomesPermission, deleteIncomesPermission);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default permissions created successfully");
                }

                if (!await _context.Roles.AnyAsync())
                {
                    _logger.LogInformation("Creating default roles...");

                    var adminRole = new Role("Admin", "Administrator role with full access");
                    var userRole = new Role("User", "Standard user role with minimal access");

                    await _context.Roles.AddRangeAsync(adminRole, userRole);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default roles created successfully");

                    _logger.LogInformation("Assigning permissions to roles...");

                    var allPermissions = await _context.Permissions.ToListAsync();

                    foreach (var permission in allPermissions)
                    {
                        adminRole.AddPermission(permission);
                    }

                    var userPermissions = allPermissions
                        .Where(p => p.SystemName == "users.view").ToList();

                    userPermissions.AddRange(allPermissions.Where(p =>
                        p.SystemName.StartsWith("payments.") ||
                        p.SystemName.StartsWith("creditcards.") ||
                        p.SystemName.StartsWith("incomes.")));

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
                }

                if (!await _context.PaymentTypes.AnyAsync())
                {
                    _logger.LogInformation("Creating default payment types...");

                    var paymentTypes = new List<PaymentType>
                    {
                        new PaymentType("Alimentação", "Gastos com alimentação, restaurantes, mercado, etc."),
                        new PaymentType("Moradia", "Gastos com aluguel, condomínio, IPTU, etc."),
                        new PaymentType("Transporte", "Gastos com combustível, transporte público, manutenção de veículos, etc."),
                        new PaymentType("Saúde", "Gastos com plano de saúde, medicamentos, consultas médicas, etc."),
                        new PaymentType("Educação", "Gastos com mensalidades escolares, cursos, livros, etc."),
                        new PaymentType("Lazer", "Gastos com cinema, viagens, eventos, etc."),
                        new PaymentType("Vestuário", "Gastos com roupas, calçados, acessórios, etc."),
                        new PaymentType("Serviços Públicos", "Gastos com água, luz, gás, internet, telefone, etc."),
                        new PaymentType("Impostos", "Gastos com impostos e taxas governamentais"),
                        new PaymentType("Investimentos", "Gastos com aplicações financeiras, ações, etc."),
                        new PaymentType("Outros", "Gastos diversos não classificados em outras categorias")
                    };

                    await _context.PaymentTypes.AddRangeAsync(paymentTypes);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default payment types created successfully");
                }

                if (!await _context.PaymentMethods.AnyAsync())
                {
                    _logger.LogInformation("Creating default payment methods...");

                    var paymentMethods = new List<PaymentMethod>
                    {
                        new PaymentMethod("Dinheiro", "Pagamento em espécie", PaymentMethodType.Cash),
                        new PaymentMethod("Cartão de Crédito", "Pagamento com cartão de crédito", PaymentMethodType.CreditCard),
                        new PaymentMethod("Cartão de Débito", "Pagamento com cartão de débito", PaymentMethodType.DebitCard),
                        new PaymentMethod("Transferência Bancária", "Pagamento por transferência bancária", PaymentMethodType.BankTransfer),
                        new PaymentMethod("PIX", "Pagamento via PIX", PaymentMethodType.BankTransfer),
                        new PaymentMethod("Boleto Bancário", "Pagamento por boleto bancário", PaymentMethodType.BankTransfer),
                        new PaymentMethod("Cheque", "Pagamento com cheque", PaymentMethodType.Check),
                        new PaymentMethod("PayPal", "Pagamento via PayPal", PaymentMethodType.DigitalWallet),
                        new PaymentMethod("Google Pay", "Pagamento via Google Pay", PaymentMethodType.DigitalWallet),
                        new PaymentMethod("Apple Pay", "Pagamento via Apple Pay", PaymentMethodType.DigitalWallet),
                        new PaymentMethod("Mercado Pago", "Pagamento via Mercado Pago", PaymentMethodType.DigitalWallet),
                        new PaymentMethod("PicPay", "Pagamento via PicPay", PaymentMethodType.DigitalWallet),
                        new PaymentMethod("Outro", "Outros métodos de pagamento", PaymentMethodType.Other)
                    };

                    await _context.PaymentMethods.AddRangeAsync(paymentMethods);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Default payment methods created successfully");
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