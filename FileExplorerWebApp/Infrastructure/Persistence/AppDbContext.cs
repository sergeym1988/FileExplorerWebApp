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

        public DbSet<Folder> Folders => Set<Folder>();
        public DbSet<Domain.Entities.File> FileItems => base.Set<Domain.Entities.File>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Folder>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity
                    .HasMany(f => f.SubFolders)
                    .WithOne(f => f.ParentFolder)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasMany(f => f.Files)
                    .WithOne(f => f.Folder)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(f => f.Name).IsRequired();
            });

            modelBuilder.Entity<Domain.Entities.File>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.Property(f => f.Name).IsRequired();
                entity.Property(f => f.Mime).IsRequired();
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
