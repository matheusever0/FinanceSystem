using FinanceSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Infrastructure.Data
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
        }
    }
}