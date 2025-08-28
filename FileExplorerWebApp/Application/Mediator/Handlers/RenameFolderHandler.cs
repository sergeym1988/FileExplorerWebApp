using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Commands;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    /// <summary>
    /// The rename folder handler.
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;FileExplorerWebApp.Application.Mediator.Commands.FolderCommands.RenameFolderCommand, System.Boolean&gt;" />
    public class RenameFolderHandler : IRequestHandler<FolderCommands.RenameFolderCommand, bool>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger<RenameFolderHandler> _logger;

        public RenameFolderHandler(
            IRepositoryWrapper repositoryWrapper,
            ILogger<RenameFolderHandler> logger
        )
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<bool> Handle(
            FolderCommands.RenameFolderCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var folder = await _repositoryWrapper.Folders.FindByIdAsync(request.FolderDto.Id);
                if (folder == null)
                {
                    return false;
                }

                folder.Name = request.FolderDto.Name;
                folder.ParentFolderId = request.FolderDto.ParentFolderId;
                folder.LastModifiedDateTime = DateTime.UtcNow;

                _repositoryWrapper.Folders.Update(folder);
                await _repositoryWrapper.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while renaming folder with ID {FolderId}",
                    request.FolderDto.Id
                );
                return false;
            }
        }
    }
}
