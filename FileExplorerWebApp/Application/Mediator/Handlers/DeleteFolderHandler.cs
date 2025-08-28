using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Commands;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class DeleteFolderHandler : IRequestHandler<FolderCommands.DeleteFolderCommand, bool>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger<DeleteFolderHandler> _logger;

        public DeleteFolderHandler(
            IRepositoryWrapper repositoryWrapper,
            ILogger<DeleteFolderHandler> logger
        )
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<bool> Handle(
            FolderCommands.DeleteFolderCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var folder = await _repositoryWrapper.Folders.FindByIdAsync(request.FolderId);
                if (folder == null)
                {
                    return false;
                }

                _repositoryWrapper.Folders.Delete(folder);
                await _repositoryWrapper.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting folder with ID {FolderId}",
                    request.FolderId
                );
                return false;
            }
        }
    }
}
