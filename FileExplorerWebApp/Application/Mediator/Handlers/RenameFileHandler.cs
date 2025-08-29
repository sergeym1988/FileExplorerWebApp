using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Commands;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    /// <summary>
    /// The rename file handler.
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;FileExplorerWebApp.Application.Mediator.Commands.FileCommands.RenameFileCommand, System.Boolean&gt;" />
    public class RenameFileHandler : IRequestHandler<FileCommands.RenameFileCommand, bool>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger<RenameFileHandler> _logger;

        public RenameFileHandler(
            IRepositoryWrapper repositoryWrapper,
            ILogger<RenameFileHandler> logger
        )
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<bool> Handle(
            FileCommands.RenameFileCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var file = await _repositoryWrapper.Files.FindByIdAsync(request.FileDto.Id);
                if (file == null)
                {
                    return false;
                }

                file.Name = request.FileDto.Name ?? string.Empty;
                file.LastModifiedDateTime = DateTime.UtcNow;

                _repositoryWrapper.Files.Update(file);
                await _repositoryWrapper.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while renaming file with ID {FileId}",
                    request.FileDto.Id
                );
                return false;
            }
        }
    }
}
