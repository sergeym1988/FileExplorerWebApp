using FileExplorerWebApp.Domain.Entities;

namespace FileExplorerWebApp.Application.Interfaces.Repositories
{
    public interface IFolderRepository : IRepositoryBase<Folder>
    {
        Task<Folder?> GetFolderWithChildrenAsync(Guid id);
        Task<IEnumerable<Folder>> GetRootFoldersAsync();
        Task<IEnumerable<Folder>> GetChildrenFoldersAsync(Guid parentId);
        Task<Folder?> FindByIdAsync(Guid id);
    }
}
