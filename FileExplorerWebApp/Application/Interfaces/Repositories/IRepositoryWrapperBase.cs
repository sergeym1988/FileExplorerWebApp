namespace FileExplorerWebApp.Application.Interfaces.Repositories
{
    public interface IRepositoryWrapperBase
    {
        Task SaveAsync();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
    }
}
