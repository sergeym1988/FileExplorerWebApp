using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Commands;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class RenameFolderHandler
        : IRequestHandler<FolderCommands.RenameFolderCommand, bool>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public RenameFolderHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<bool> Handle(
            FolderCommands.RenameFolderCommand request,
            CancellationToken cancellationToken
        )
        {
            var folder = await _repositoryWrapper.Folders.FindByIdAsync(request.FolderDto.Id);
            if (folder == null)
                return false;

            folder.Name = request.FolderDto.Name;
            folder.ParentFolderId = request.FolderDto.ParentFolderId;

            _repositoryWrapper.Folders.Update(folder);
            await _repositoryWrapper.SaveAsync();

            return true;
        }
    }
}
