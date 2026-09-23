using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Entities.Contexts
{
    /// <summary>
    /// Le contexte qui represente les tables de notre base de donnee
    /// </summary>
    /// <param name="options"></param>
    public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Comunity> Comunities => Set<Comunity>();
        public DbSet<ComunityMember> ComunityMembers => Set<ComunityMember>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<Signal> Signals => Set<Signal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Comunity>().ToTable("Comunities").HasKey(c => c.Id);
            modelBuilder.Entity<Comunity>().Ignore(c => c.Users).Ignore(c => c.PostsList);
            modelBuilder.Entity<ComunityMember>().ToTable("ComunityMembers").HasKey(m => new { m.ComunityId, m.UserId });
            modelBuilder.Entity<ComunityMember>().HasOne<User>().WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ComunityMember>().HasOne<Comunity>().WithMany().HasForeignKey(m => m.ComunityId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().ToTable("Posts");
            modelBuilder.Entity<Post>().Property(p => p.ImagesPath).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            modelBuilder.Entity<Post>().Property(p => p.AdditionalsPath).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            modelBuilder.Entity<Post>().HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Post>().HasOne<Comunity>().WithMany().HasForeignKey(p => p.CommunityId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Report>().ToTable("Reports");
            modelBuilder.Entity<Signal>().ToTable("Signals");
        }
    }
}
