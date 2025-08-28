using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Commands;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class DeleteFileHandler
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger<DeleteFileHandler> _logger;

        public DeleteFileHandler(
            IRepositoryWrapper repositoryWrapper,
            ILogger<DeleteFileHandler> logger
        )
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<bool> Handle(
            FileCommands.DeleteFileCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var file = await _repositoryWrapper.Files.FindByIdAsync(request.FileId);
                if (file == null)
                    return false;

                _repositoryWrapper.Files.Delete(file);

                await _repositoryWrapper.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while deleting file with ID {FileId}",
                    request.FileId
                );
                return false;
            }
        }
    }
}
