using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Commands;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class DeleteFileHandler : IRequestHandler<FileCommands.DeleteFileCommand, bool>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public DeleteFileHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<bool> Handle(
            FileCommands.DeleteFileCommand request,
            CancellationToken cancellationToken
        )
        {
            var file = await _repositoryWrapper.Files.FindByIdAsync(request.FileId);
            if (file == null)
                return false;

            _repositoryWrapper.Files.Delete(file);
            await _repositoryWrapper.SaveAsync();

            return true;
        }
    }
}
