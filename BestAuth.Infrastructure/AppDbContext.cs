using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BestAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace BestAuth.Infrastructure
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Request> Requests { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().Property(u => u.Name).HasMaxLength(256);

            builder.Entity<Request>(entity =>
            {
                entity.Property(r => r.ClientName).HasMaxLength(256).IsRequired();
                entity.Property(r => r.Phone).HasMaxLength(64).IsRequired();
                entity.Property(r => r.Comment).HasMaxLength(1000);
                entity.Property(r => r.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();
                entity.Property(r => r.CreatedAtUtc).IsRequired();
            });
        }
    }
}
