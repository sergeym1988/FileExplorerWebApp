using FileExplorerWebApp.Application.Interfaces.Repositories;
using MediatR;
using static FileExplorerWebApp.Application.Mediator.Commands.FolderCommands;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    /// <summary>
    /// The delete folder handler.
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;FileExplorerWebApp.Application.Mediator.Commands.FolderCommands.DeleteFolderCommand, System.Boolean&gt;" />
    public class DeleteFolderHandler : IRequestHandler<DeleteFolderCommand, bool>
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
            DeleteFolderCommand request,
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
