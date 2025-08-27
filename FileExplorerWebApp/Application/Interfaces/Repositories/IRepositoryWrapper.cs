namespace FileExplorerWebApp.Application.Interfaces.Repositories
{
    public interface IRepositoryWrapper : IRepositoryWrapperBase
    {
        IFolderRepository Folders { get; }
        IFileRepository Files { get; }
    }
}
