using AutoMapper;
using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    /// <summary>
    /// Handler for retrieving subfolders by parent folder id.
    /// </summary>
    public class GetSubfoldersByIdHandler
        : IRequestHandler<FolderQueries.GetSubfoldersByIdQuery, List<FolderDto>?>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILogger<GetSubfoldersByIdHandler> _logger;

        public GetSubfoldersByIdHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ILogger<GetSubfoldersByIdHandler> logger
        )
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<FolderDto>?> Handle(
            FolderQueries.GetSubfoldersByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                Guid? parentId = request.FolderId == Guid.Empty ? null : request.FolderId;

                var parentFolder = parentId.HasValue
                    ? await _repositoryWrapper.Folders.GetFolderByIdAsync(
                        parentId.Value,
                        cancellationToken
                    )
                    : null;

                if (parentId.HasValue && parentFolder == null)
                    return new List<FolderDto>();

                var childFolders = await _repositoryWrapper.Folders.GetChildFoldersAsync(
                    parentId,
                    cancellationToken
                );
                var childIds = childFolders.Select(f => f.Id).ToList();

                var subfolderCounts = await _repositoryWrapper.Folders.GetSubfolderCountsAsync(
                    childIds,
                    cancellationToken
                );

                var childDtos = _mapper.Map<List<FolderDto>>(childFolders);

                foreach (var dto in childDtos)
                {
                    dto.HasChildren = subfolderCounts.ContainsKey(dto.Id);
                    dto.SubFolders = null;
                    dto.Files = null;
                }

                var parentDto = new FolderDto
                {
                    Id = request.FolderId,
                    Name = parentFolder?.Name ?? "Root",
                    ParentFolderId = parentFolder?.ParentFolderId,
                    HasChildren = childDtos.Any(),
                    SubFolders = childDtos,
                    Files = null,
                    CreatedDateTime = parentFolder?.CreatedDateTime,
                    LastModifiedDateTime = parentFolder?.LastModifiedDateTime,
                };

                return new List<FolderDto> { parentDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while retrieving folder with ID {FolderId}",
                    request.FolderId
                );
                return null;
            }
        }
    }
}
