using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValueSql("gen_random_bytes(8)");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_users_first_name_length", "LENGTH(\"first_name\") >= 2");
            t.HasCheckConstraint("chk_users_last_name_length", "LENGTH(\"last_name\") >= 2");
            t.HasCheckConstraint("chk_users_email_length", "LENGTH(\"email\") >= 7");
            t.HasCheckConstraint("chk_users_password_hash_length", "LENGTH(\"password_hash\") >= 60");
        });

        builder.HasMany(u => u.Properties)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Bookings)
            .WithOne(b => b.Tenant)
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Reviews)
            .WithOne(r => r.Reviewer)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}