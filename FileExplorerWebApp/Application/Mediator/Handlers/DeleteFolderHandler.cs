using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Commands;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class DeleteFolderHandler
        : IRequestHandler<FolderCommands.DeleteFolderCommand, bool>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public DeleteFolderHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<bool> Handle(
            FolderCommands.DeleteFolderCommand request,
            CancellationToken cancellationToken
        )
        {
            var folder = await _repositoryWrapper.Folders.FindByIdAsync(request.FolderId);
            if (folder == null)
                return false;

            _repositoryWrapper.Folders.Delete(folder);
            await _repositoryWrapper.SaveAsync();

            return true;
        }
    }
}
