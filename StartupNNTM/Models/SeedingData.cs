using Microsoft.EntityFrameworkCore;
using StartupNNTM.Models;

public class SeedingData
{
    private readonly ModelBuilder modelBuilder;

    public SeedingData(ModelBuilder modelBuilder)
    {
        this.modelBuilder = modelBuilder;
    }

    public void Seed()
    {
        // ADMINISTRATOR
        var adminId = new Guid("509450FA-7E51-4FBC-BAA2-CD3B2BFFFA91");
        var userId = new Guid("74A4E0A7-6E10-4810-BAEF-127F62E72E59");

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = adminId,
                Name = "admin",
                NormalizedName = "admin"
            },
            new Role
            {
                Id = userId,
                Name = "user",
                NormalizedName = "user"
            });


    }
}