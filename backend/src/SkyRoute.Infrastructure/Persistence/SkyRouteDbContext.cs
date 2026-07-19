using Microsoft.EntityFrameworkCore;
using SkyRoute.Core.Entities;

namespace SkyRoute.Infrastructure.Persistence;

public class SkyRouteDbContext(DbContextOptions<SkyRouteDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<Passenger> Passengers => Set<Passenger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SkyRouteDbContext).Assembly);
    }
}
