using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("listings");
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => l.PropertyId);
        builder.HasIndex(l => new { l.Status, l.Price });

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValueSql("gen_random_bytes(8)");

        builder.Property(l => l.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(l => l.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(l => l.MinRentalPeriodInMonths)
            .IsRequired();

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_listings_price_positive", "\"price\" > 0");
            t.HasCheckConstraint("chk_listings_min_period_positive", "\"min_rental_period_in_months\" > 0");
        });

        builder.HasOne(l => l.Property)
            .WithMany(p => p.Listings)
            .HasForeignKey(l => l.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Bookings)
            .WithOne(b => b.Listing)
            .HasForeignKey(b => b.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}