using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileExplorerWebApp.Infrastructure.Repositories
{
    /// <summary>
    /// The file repository.
    /// </summary>
    /// <seealso cref="FileExplorerWebApp.Infrastructure.Repositories.RepositoryBase&lt;FileExplorerWebApp.Domain.Entities.File&gt;" />
    /// <seealso cref="FileExplorerWebApp.Application.Interfaces.Repositories.IFileRepository" />
    public class FileRepository : RepositoryBase<Domain.Entities.File>, IFileRepository
    {
        public FileRepository(AppDbContext context)
            : base(context) { }

        /// <summary>
        /// Gets the files by folder identifier asynchronous.
        /// </summary>
        /// <param name="folderId">The folder identifier.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Domain.Entities.File>> GetFilesByFolderIdAsync(
            Guid folderId
        ) => await _context.Files.AsNoTracking().Where(fi => fi.FolderId == folderId).ToListAsync();

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<Domain.Entities.File?> FindByIdAsync(Guid id) =>
            await _context.Files.AsNoTracking().FirstOrDefaultAsync(fi => fi.Id == id);

        /// <summary>
        /// Gets the files by folder ids asynchronous.
        /// </summary>
        /// <param name="folderIds">The folder ids.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<List<Domain.Entities.File>> GetFilesByFolderIdsAsync(
            IEnumerable<Guid> folderIds,
            CancellationToken ct
        )
        {
            if (!folderIds.Any())
                return new List<Domain.Entities.File>();

            return await _context
                .Files.AsNoTracking()
                .Where(f => f.FolderId != null && folderIds.Contains(f.FolderId.Value))
                .ToListAsync(ct);
        }

        /// <summary>
        /// Gets the file counts by folder ids asynchronous.
        /// </summary>
        /// <param name="folderIds">The folder ids.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<Dictionary<Guid, int>> GetFileCountsByFolderIdsAsync(
            IEnumerable<Guid> folderIds,
            CancellationToken ct
        )
        {
            if (!folderIds.Any())
                return new Dictionary<Guid, int>();

            return await _context
                .Files.AsNoTracking()
                .Where(f => f.FolderId != null && folderIds.Contains(f.FolderId.Value))
                .GroupBy(f => f.FolderId)
                .Select(g => new { FolderId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.FolderId!.Value, g => g.Count, ct);
        }

        /// <summary>
        /// Gets the root files asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Domain.Entities.File>> GetRootFilesAsync() =>
            await _context.Files.Where(fi => fi.FolderId == null).ToListAsync();
    }
}
