using FileExplorerWebApp.Domain.Entities;

namespace FileExplorerWebApp.Application.Interfaces.Repositories
{
    public interface IFolderRepository : IRepositoryBase<Folder>
    {
        /// <summary>
        /// Gets the folder with children asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<Folder?> GetFolderWithChildrenAsync(Guid id);

        /// <summary>
        /// Gets the root folders asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Folder>> GetRootFoldersAsync();

        /// <summary>
        /// Gets the children folders asynchronous.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        /// <returns></returns>
        Task<IEnumerable<Folder>> GetChildrenFoldersAsync(Guid parentId);

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<Folder?> FindByIdAsync(Guid id);

        /// <summary>
        /// Gets the folder by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<Folder?> GetFolderByIdAsync(Guid id, CancellationToken ct);

        /// <summary>
        /// Gets the child folders asynchronous.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<List<Folder>> GetChildFoldersAsync(Guid? parentId, CancellationToken ct);

        /// <summary>
        /// Gets the subfolder counts asynchronous.
        /// </summary>
        /// <param name="parentIds">The parent ids.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<Dictionary<Guid, int>> GetSubfolderCountsAsync(
            IEnumerable<Guid> parentIds,
            CancellationToken ct
        );

        /// <summary>
        /// Gets the file counts asynchronous.
        /// </summary>
        /// <param name="folderIds">The folder ids.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<Dictionary<Guid, int>> GetFileCountsAsync(
            IEnumerable<Guid> folderIds,
            CancellationToken ct
        );
    }
}
