using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entities;

namespace Product.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Auditable / Soft delete
        builder.Property(c => c.CreatedOnUtc).HasColumnName("CreatedOnUtc").IsRequired();
        builder.Property(c => c.ModifiedOnUtc).HasColumnName("ModifiedOnUtc");
        builder.Property(c => c.Deleted).HasColumnName("Deleted").HasDefaultValue(false);
        builder.Property(c => c.DeletedOnUtc).HasColumnName("DeletedOnUtc");

        // Global query filter for soft-deleted categories
        builder.HasQueryFilter(c => !c.Deleted);

        // Configure one-to-many relationship with Product
        builder.HasMany(c => c.Products)
               .WithOne()
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

    }
}
