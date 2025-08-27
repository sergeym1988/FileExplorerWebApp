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
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderRepository"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
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
            await _context.Folders.SingleOrDefaultAsync(f => f.Id == id);
    }
}
