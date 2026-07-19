using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyRoute.Core.Entities;
using SkyRoute.Core.Enums;

namespace SkyRoute.Infrastructure.Persistence.Configurations;

public class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
{
    public void Configure(EntityTypeBuilder<Passenger> builder)
    {
        builder.ToTable("Passengers");

        builder.HasKey(passenger => passenger.Id);

        builder.Property(passenger => passenger.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(passenger => passenger.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(passenger => passenger.DocumentNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(passenger => passenger.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
    }
}
