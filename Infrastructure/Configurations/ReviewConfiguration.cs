using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.PropertyId);
        builder.HasIndex(r => r.ReviewerId);
        builder.HasIndex(r => r.Rating);

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValueSql("gen_random_bytes(8)");

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasColumnType("text");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_reviews_rating_range", "\"rating\" >= 1 AND \"rating\" <= 5");
            t.HasCheckConstraint("chk_reviews_comment_length", "LENGTH(\"comment\") >= 10");
        });

        builder.HasOne(r => r.Reviewer)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Property)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}