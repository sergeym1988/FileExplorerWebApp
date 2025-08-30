namespace FileExplorerWebApp.Application.Interfaces.Repositories
{
    public interface IFileRepository : IRepositoryBase<Domain.Entities.File>
    {
        /// <summary>
        /// Gets the files by folder identifier asynchronous.
        /// </summary>
        /// <param name="folderId">The folder identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<Domain.Entities.File>> GetFilesByFolderIdAsync(
            Guid folderId,
            CancellationToken ct = default
        );

        /// <summary>
        /// Gets the root files asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<IEnumerable<Domain.Entities.File>> GetRootFilesAsync(CancellationToken ct = default);

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<Domain.Entities.File?> FindByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Gets the files by folder ids asynchronous.
        /// </summary>
        /// <param name="folderIds">The folder ids.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<List<Domain.Entities.File>> GetFilesByFolderIdsAsync(
            IEnumerable<Guid> folderIds,
            CancellationToken ct = default
        );

        /// <summary>
        /// Gets the file counts by folder ids asynchronous.
        /// </summary>
        /// <param name="folderIds">The folder ids.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<Dictionary<Guid, int>> GetFileCountsByFolderIdsAsync(
            IEnumerable<Guid> folderIds,
            CancellationToken ct = default
        );
    }
}
