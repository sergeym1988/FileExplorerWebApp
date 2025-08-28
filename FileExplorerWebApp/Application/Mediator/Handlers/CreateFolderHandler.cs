using AutoMapper;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Domain.Entities;
using MediatR;
using static FileExplorerWebApp.Application.Mediator.Commands.FolderCommands;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class CreateFolderHandler : IRequestHandler<CreateFolderCommand, bool>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILogger<UploadFilesHandler> _logger;

        public CreateFolderHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ILogger<UploadFilesHandler> logger
        )
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> Handle(
            CreateFolderCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var folder = _mapper.Map<Folder>(request.FolderDto);
                folder.ParentFolderId =
                    folder.ParentFolderId == Guid.Empty ? null : folder.ParentFolderId;
                folder.CreatedDateTime = DateTime.UtcNow;

                await _repositoryWrapper.Folders.CreateAsync(folder);

                await _repositoryWrapper.SaveAsync();
                request.FolderDto.Id = folder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating folder");
                return false;
            }

            return true;
        }
    }
}
