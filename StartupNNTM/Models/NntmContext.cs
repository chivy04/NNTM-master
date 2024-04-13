using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace StartupNNTM.Models
{
    public class NntmContext : IdentityDbContext<User, Role, Guid>
    {
        public NntmContext() { }

        public NntmContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> User {  get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Message> Messages { get; set; }    
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Type> Types { get; set; }
        public DbSet<EmailGetCode> EmailGetCodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(SystemConstants.ConnectString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Message>(entity =>
            {
                entity.HasOne(e => e.User).WithMany(e => e.Message).HasForeignKey(e => e.From).HasConstraintName("FK__User__From__06CD04F7");
                entity.HasOne(e => e.User).WithMany(e => e.Message).HasForeignKey(e => e.To).HasConstraintName("FK__User__To__06CD04F7");
            });

            builder.Entity<Chat>(entity =>
            {
                entity.HasOne(e => e.Message).WithMany(e => e.Chat).HasForeignKey(e => e.MessageId).HasConstraintName("FK__Message__Id__06CD04F7");
                entity.HasOne(e => e.Post).WithMany(e => e.Chat).HasForeignKey(e => e.PostId).HasConstraintName("FK__Post__Id__06CD04F7");
            });

            builder.Entity<Post>(entity =>
            {
                entity.HasOne(e => e.Type).WithMany(e => e.Post).HasForeignKey(e => e.TypeId);
                entity.HasOne(e => e.Address).WithMany(e => e.Post).HasForeignKey(e => e.AddressId);
                entity.HasMany(e => e.Images).WithOne(e => e.Post).HasForeignKey(e => e.PostId);
            });


            builder.Entity<IdentityUserRole<Guid>>().HasKey(x => new { x.UserId, x.RoleId });

            new SeedingData(builder).Seed();
        }
    }
}
