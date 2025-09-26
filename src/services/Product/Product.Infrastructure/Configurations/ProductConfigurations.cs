using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Product.Domain.Entities;

namespace Product.Infrastructure.Configurations;

public class ProductConfigurations : IEntityTypeConfiguration<Domain.Entities.Product>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasColumnName("Name").IsRequired();

        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.WithOwner();

            priceBuilder.Property(price => price.Amount)
                .HasColumnName("PriceAmount")
                .IsRequired();

            priceBuilder.Property(price => price.Currency)
                .HasColumnName("PriceCurrency")
                .IsRequired();
        });

        builder.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId);

        builder.Property(p => p.CreatedOnUtc).HasColumnName("CreatedOnUtc").IsRequired();

        builder.Property(p => p.ModifiedOnUtc).HasColumnName("ModifiedOnUtc");

        builder.Property(p => p.DeletedOnUtc).HasColumnName("DeletedOnUtc");

        builder.Property(p=>p.Deleted).HasDefaultValue(false);

        builder.HasIndex(p => p.Name);

        builder.HasIndex(p => p.CategoryId);

        builder.HasQueryFilter(p => !p.Deleted);
    }
}
