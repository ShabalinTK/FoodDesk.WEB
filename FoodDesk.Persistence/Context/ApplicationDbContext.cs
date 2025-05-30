 using FoodDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodDesk.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var admin = new IdentityRole
        {
            Id = "1",
            Name = "admin",
            NormalizedName = "ADMIN"
        };

        var client = new IdentityRole
        {
            Id = "2",
            Name = "client",
            NormalizedName = "CLIENT"
        };

        var courier = new IdentityRole
        {
            Id = "3",
            Name = "courier",
            NormalizedName = "COURIER"
        };

        modelBuilder.Entity<IdentityRole>().HasData(admin, client, courier);
    }

}