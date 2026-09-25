using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Entities.Contexts
{
    /// <summary>
    /// Le contexte qui represente les tables de notre base de donnee
    /// </summary>
    /// <param name="options"></param>
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public virtual DbSet<User> Users => Set<User>();
        public virtual DbSet<Comunity> Comunities => Set<Comunity>();
        public virtual DbSet<ComunityMember> ComunityMembers => Set<ComunityMember>();
        public virtual DbSet<Post> Posts => Set<Post>();
        public virtual DbSet<Report> Reports => Set<Report>();
        public virtual DbSet<Signal> Signals => Set<Signal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // The Users table
            modelBuilder.Entity<User>().ToTable("Users").HasKey(user => user.UserId);
            modelBuilder.Entity<User>().Property(user => user.Email).IsRequired().HasMaxLength(254);
            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique()
                .HasDatabaseName("UX_Users_Email");

            // The Comunities table
            modelBuilder.Entity<Comunity>().ToTable("Comunities").HasKey(c => c.Id);
            modelBuilder.Entity<Comunity>().Ignore(c => c.Users).Ignore(c => c.PostsList);

            // The ComunityMembers table
            // A member can join a community only once; the composite primary key enforces this.
            modelBuilder.Entity<ComunityMember>().ToTable("ComunityMembers").HasKey(m => new { m.ComunityId, m.UserId });
            modelBuilder.Entity<ComunityMember>().HasOne<User>().WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ComunityMember>().HasOne<Comunity>().WithMany().HasForeignKey(m => m.ComunityId).OnDelete(DeleteBehavior.Cascade);

            // The Posts table
            modelBuilder.Entity<Post>().ToTable("Posts").HasKey(post => post.Id);
            modelBuilder.Entity<Post>().Property(p => p.ImagesPath).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            modelBuilder.Entity<Post>().Property(p => p.AdditionalsPath).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            modelBuilder.Entity<Post>().HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Post>().HasOne<Comunity>().WithMany().HasForeignKey(p => p.CommunityId).OnDelete(DeleteBehavior.Cascade);

            // The Reports table
            // A post may have multiple reports, so only each report's Id is unique.
            modelBuilder.Entity<Report>().ToTable("Reports").HasKey(report => report.Id);

            // The Signals table
            modelBuilder.Entity<Signal>().ToTable("Signals").HasKey(signal => signal.Id);
        }
    }
}
