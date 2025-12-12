using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Location);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValueSql("gen_random_bytes(8)");

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(p => p.Location)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.PropertyType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_properties_title_length", "LENGTH(\"title\") >= 5");
            t.HasCheckConstraint("chk_properties_description_length", "LENGTH(\"description\") >= 10");
            t.HasCheckConstraint("chk_properties_location_length", "LENGTH(\"location\") >= 5");
        });

        builder.HasOne(p => p.User)
            .WithMany(u => u.Properties)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Listings)
            .WithOne(l => l.Property)
            .HasForeignKey(l => l.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PropertyAmenities)
            .WithOne(pa => pa.Property)
            .HasForeignKey(pa => pa.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Reviews)
            .WithOne(r => r.Property)
            .HasForeignKey(r => r.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}