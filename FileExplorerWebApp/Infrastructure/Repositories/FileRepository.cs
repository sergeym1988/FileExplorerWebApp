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
        /// <summary>
        /// Initializes a new instance of the <see cref="FileRepository"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public FileRepository(AppDbContext context)
            : base(context) { }

        /// <summary>
        /// Gets the files by folder identifier asynchronous.
        /// </summary>
        /// <param name="folderId">The folder identifier.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Domain.Entities.File>> GetFilesByFolderIdAsync(
            Guid folderId
        ) => await _context.FileItems.Where(fi => fi.FolderId == folderId).ToListAsync();

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<Domain.Entities.File?> FindByIdAsync(Guid id) =>
            await _context.FileItems.FirstOrDefaultAsync(fi => fi.Id == id);
    }
}
