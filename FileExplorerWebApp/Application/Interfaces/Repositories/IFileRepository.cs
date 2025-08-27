namespace FileExplorerWebApp.Application.Interfaces.Repositories
{
    public interface IFileRepository : IRepositoryBase<Domain.Entities.File>
    {
        Task<IEnumerable<Domain.Entities.File>> GetFilesByFolderIdAsync(Guid folderId);

        Task<Domain.Entities.File?> FindByIdAsync(Guid id);
    }
}
