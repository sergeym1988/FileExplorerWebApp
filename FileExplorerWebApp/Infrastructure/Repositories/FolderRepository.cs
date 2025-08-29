using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Domain.Entities;
using FileExplorerWebApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileExplorerWebApp.Infrastructure.Repositories
{
    /// <summary>
    /// The folder repository
    /// </summary>
    /// <seealso cref="FileExplorerWebApp.Infrastructure.Repositories.RepositoryBase&lt;FileExplorerWebApp.Domain.Entities.Folder&gt;" />
    /// <seealso cref="FileExplorerWebApp.Application.Interfaces.Repositories.IFolderRepository" />
    public class FolderRepository : RepositoryBase<Folder>, IFolderRepository
    {
        public FolderRepository(AppDbContext context)
            : base(context) { }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<Folder?> FindByIdAsync(Guid id) =>
            await _context.Folders.SingleOrDefaultAsync(f => f.Id == id);

        /// <summary>
        /// Gets the root folders asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Folder>> GetRootFoldersAsync() =>
            await _context.Folders.Where(f => f.ParentFolderId == null).ToListAsync();

        /// <summary>
        /// Gets the children folders asynchronous.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Folder>> GetChildrenFoldersAsync(Guid parentId) =>
            await _context.Folders.Where(f => f.ParentFolderId == parentId).ToListAsync();

        /// <summary>
        /// Gets the folder with children asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<Folder?> GetFolderWithChildrenAsync(Guid id) =>
            await _context
                .Folders.Include(f => f.SubFolders)
                .Include(f => f.Files)
                .SingleOrDefaultAsync(f => f.Id == id);

        /// <summary>
        /// Gets the folder by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<Folder?> GetFolderByIdAsync(Guid id, CancellationToken ct) =>
            await _context.Folders.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id, ct);

        /// <summary>
        /// Gets the child folders asynchronous.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<List<Folder>> GetChildFoldersAsync(
            Guid? parentId,
            CancellationToken ct
        ) =>
            await _context
                .Folders.AsNoTracking()
                .Where(f => f.ParentFolderId == parentId)
                .ToListAsync(ct);

        /// <summary>
        /// Gets the subfolder counts asynchronous.
        /// </summary>
        /// <param name="parentIds">The parent ids.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<Dictionary<Guid, int>> GetSubfolderCountsAsync(
            IEnumerable<Guid> parentIds,
            CancellationToken ct
        )
        {
            if (!parentIds.Any())
                return new Dictionary<Guid, int>();

            return await _context
                .Folders.AsNoTracking()
                .Where(f => f.ParentFolderId != null && parentIds.Contains(f.ParentFolderId.Value))
                .GroupBy(f => f.ParentFolderId)
                .Select(g => new { ParentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.ParentId!.Value, g => g.Count, ct);
        }

        /// <summary>
        /// Gets the file counts asynchronous.
        /// </summary>
        /// <param name="folderIds">The folder ids.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<Dictionary<Guid, int>> GetFileCountsAsync(
            IEnumerable<Guid> folderIds,
            CancellationToken ct
        )
        {
            if (!folderIds.Any())
                return new Dictionary<Guid, int>();

            return await _context
                .Set<Domain.Entities.File>()
                .AsNoTracking()
                .Where(f => f.FolderId != null && folderIds.Contains(f.FolderId.Value))
                .GroupBy(f => f.FolderId)
                .Select(g => new { FolderId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.FolderId!.Value, g => g.Count, ct);
        }
    }
}
