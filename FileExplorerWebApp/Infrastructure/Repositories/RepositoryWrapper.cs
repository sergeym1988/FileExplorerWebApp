using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Infrastructure.Persistence;

namespace FileExplorerWebApp.Infrastructure.Repositories
{
    /// <summary>
    /// The repository wrapper.
    /// </summary>
    /// <seealso cref="FileExplorerWebApp.Application.Interfaces.Repositories.IRepositoryWrapper" />
    public class RepositoryWrapper : IRepositoryWrapper
    {
        /// <summary>
        /// The context
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Gets the folders repository.
        /// </summary>
        /// <value>
        /// The folders.
        /// </value>
        public IFolderRepository Folders { get; }

        /// <summary>
        /// Gets the files repository.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        public IFileRepository Files { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryWrapper"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="folders">The folders.</param>
        /// <param name="fileItems">The file items.</param>
        public RepositoryWrapper(
            AppDbContext context,
            IFolderRepository folders,
            IFileRepository fileItems
        )
        {
            _context = context;
            Folders = folders;
            Files = fileItems;
        }

        /// <summary>
        /// Saves the asynchronous.
        /// </summary>
        public async Task SaveAsync() => await _context.SaveChangesAsync();

        /// <summary>
        /// Begins the transaction asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync() =>
            await _context.Database.BeginTransactionAsync();
    }
}
