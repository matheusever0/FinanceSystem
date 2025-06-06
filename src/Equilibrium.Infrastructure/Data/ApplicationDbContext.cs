using Equilibrium.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Equilibrium.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<PaymentInstallment> PaymentInstallments { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<IncomeInstallment> IncomeInstallments { get; set; }
        public DbSet<IncomeType> IncomeTypes { get; set; }
        public DbSet<Financing> Financings { get; set; }
        public DbSet<FinancingInstallment> FinancingInstallments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<User>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<Role>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<Permission>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<Payment>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<CreditCard>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<PaymentInstallment>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<PaymentMethod>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<PaymentType>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<Income>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<IncomeInstallment>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<IncomeType>()
                .Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<Financing>()
               .Property(e => e.Id)
               .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<FinancingInstallment>()
               .Property(e => e.Id)
               .HasDefaultValueSql("NEWSEQUENTIALID()");

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.SystemName)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.CreditCard)
                .WithMany()
                .HasForeignKey(p => p.CreditCardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentType)
                .WithMany(pt => pt.Payments)
                .HasForeignKey(p => p.PaymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentMethod)
                .WithMany(pm => pm.Payments)
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentType>()
                .HasOne(pt => pt.User)
                .WithMany()
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); 

            modelBuilder.Entity<PaymentMethod>()
                .HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); 

            modelBuilder.Entity<CreditCard>()
                .HasOne(cc => cc.User)
                .WithMany()
                .HasForeignKey(cc => cc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CreditCard>()
                .HasOne(cc => cc.PaymentMethod)
                .WithMany(pm => pm.CreditCards)
                .HasForeignKey(cc => cc.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentInstallment>()
                .HasOne(pi => pi.Payment)
                .WithMany(p => p.Installments)
                .HasForeignKey(pi => pi.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Income>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Income>()
                .HasOne(i => i.IncomeType)
                .WithMany(it => it.Incomes)
                .HasForeignKey(i => i.IncomeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Income>()
                .HasMany(i => i.Installments)
                .WithOne(ii => ii.Income)
                .HasForeignKey(ii => ii.IncomeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IncomeType>()
                .HasOne(it => it.User)
                .WithMany()
                .HasForeignKey(it => it.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<IncomeType>()
                .HasMany(it => it.Incomes)
                .WithOne(i => i.IncomeType)
                .HasForeignKey(i => i.IncomeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Financing>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Financing>()
                .HasMany(f => f.Installments)
                .WithOne(i => i.Financing)
                .HasForeignKey(i => i.FinancingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Financing>()
                .HasMany(f => f.Payments)
                .WithOne(p => p.Financing)
                .HasForeignKey(p => p.FinancingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinancingInstallment>()
                .HasMany(i => i.Payments)
                .WithOne(p => p.FinancingInstallment)
                .HasForeignKey(p => p.FinancingInstallmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentType>()
                .Property(pt => pt.IsFinancingType)
                .HasDefaultValue(false);
        }
    }
}