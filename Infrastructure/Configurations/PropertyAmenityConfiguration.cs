using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PropertyAmenityConfiguration : IEntityTypeConfiguration<PropertyAmenity>
{
    public void Configure(EntityTypeBuilder<PropertyAmenity> builder)
    {
        builder.ToTable("property_amenities");
        builder.HasKey(pa => pa.Id);
        builder.HasIndex(pa => new { pa.PropertyId, pa.AmenityId }).IsUnique();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValueSql("gen_random_bytes(8)");

        builder.HasOne(pa => pa.Property)
            .WithMany(p => p.PropertyAmenities)
            .HasForeignKey(pa => pa.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.Amenity)
            .WithMany(a => a.PropertyAmenities)
            .HasForeignKey(pa => pa.AmenityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}