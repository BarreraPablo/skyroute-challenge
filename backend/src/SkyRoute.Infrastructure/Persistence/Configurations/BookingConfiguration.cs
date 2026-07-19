using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyRoute.Core.Entities;

namespace SkyRoute.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.BookingReference)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(booking => booking.BookingReference)
            .IsUnique();

        builder.Property(booking => booking.FlightId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(booking => booking.Provider)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(booking => booking.Origin)
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(booking => booking.Destination)
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(booking => booking.OriginCountry)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(booking => booking.DestinationCountry)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(booking => booking.CabinClass)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(booking => booking.PricePerPassenger)
            .HasPrecision(18, 2);

        builder.Property(booking => booking.TotalPrice)
            .HasPrecision(18, 2);

        builder.HasMany(booking => booking.Passengers)
            .WithOne(passenger => passenger.Booking)
            .HasForeignKey(passenger => passenger.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
