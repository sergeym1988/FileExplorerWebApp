using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Domain.Entities;
using FileExplorerWebApp.Infrastructure.Persistence;
using FileExplorerWebApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

public class FolderRepository : RepositoryBase<Folder>, IFolderRepository
{
    public FolderRepository(AppDbContext context)
        : base(context) { }

    /// <summary>
    /// Finds the by identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<Folder?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Folders.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id, ct);

    /// <summary>
    /// Gets the root folders asynchronous.
    /// </summary>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<IEnumerable<Folder>> GetRootFoldersAsync(CancellationToken ct = default) =>
        await _context.Folders.AsNoTracking().Where(f => f.ParentFolderId == null).ToListAsync(ct);

    /// <summary>
    /// Gets the children folders asynchronous.
    /// </summary>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<IEnumerable<Folder>> GetChildrenFoldersAsync(
        Guid parentId,
        CancellationToken ct = default
    ) =>
        await _context
            .Folders.AsNoTracking()
            .Where(f => f.ParentFolderId == parentId)
            .ToListAsync(ct);

    /// <summary>
    /// Gets the folder with children asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<Folder?> GetFolderWithChildrenAsync(
        Guid id,
        CancellationToken ct = default
    ) =>
        await _context
            .Folders.Include(f => f.SubFolders)
            .Include(f => f.Files)
            .AsNoTracking()
            .SingleOrDefaultAsync(f => f.Id == id, ct);

    /// <summary>
    /// Gets the folder by identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<Folder?> GetFolderByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Folders.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id, ct);

    /// <summary>
    /// Gets the child folders asynchronous.
    /// </summary>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<List<Folder>> GetChildFoldersAsync(
        Guid? parentId,
        CancellationToken ct = default
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
        CancellationToken ct = default
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
        CancellationToken ct = default
    )
    {
        if (!folderIds.Any())
            return new Dictionary<Guid, int>();

        return await _context
            .Set<FileExplorerWebApp.Domain.Entities.File>()
            .AsNoTracking()
            .Where(f => f.FolderId != null && folderIds.Contains(f.FolderId.Value))
            .GroupBy(f => f.FolderId)
            .Select(g => new { FolderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.FolderId!.Value, g => g.Count, ct);
    }
}
