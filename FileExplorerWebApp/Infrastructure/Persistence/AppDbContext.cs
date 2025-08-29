using FileExplorerWebApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileExplorerWebApp.Infrastructure.Persistence
{
    /// <summary>
    /// The app db context
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Folder> Folders { get; set; }
        public DbSet<Domain.Entities.File> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Folder>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.Property(f => f.Name).IsRequired().HasMaxLength(255);

                entity
                    .HasMany(f => f.SubFolders)
                    .WithOne(f => f.ParentFolder)
                    .HasForeignKey(f => f.ParentFolderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasMany(f => f.Files)
                    .WithOne(f => f.Folder)
                    .HasForeignKey(f => f.FolderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Domain.Entities.File>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.Property(f => f.Name).IsRequired().HasMaxLength(255);

                entity.Property(f => f.Mime).IsRequired().HasMaxLength(100);

                entity.Property(f => f.Content).IsRequired();

                entity
                    .HasOne(f => f.Folder)
                    .WithMany(f => f.Files)
                    .HasForeignKey(f => f.FolderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
