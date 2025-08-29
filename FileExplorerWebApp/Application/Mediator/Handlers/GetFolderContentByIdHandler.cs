using AutoMapper;
using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.DTOs.Preview;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using FileExplorerWebApp.Domain.Entities;
using FileExplorerWebApp.Infrastructure.Utils;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    /// <summary>
    /// The get folder content by parent id handler.
    /// </summary>
    public class GetFolderContentByIdHandler
        : IRequestHandler<FolderQueries.GetFolderContentByIdQuery, List<FolderDto>?>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILogger<GetFolderContentByIdHandler> _logger;

        public GetFolderContentByIdHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ILogger<GetFolderContentByIdHandler> logger
        )
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<FolderDto>?> Handle(
            FolderQueries.GetFolderContentByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                Guid? parentId = request.FolderId == Guid.Empty ? null : request.FolderId;

                Folder? parentFolder = parentId.HasValue
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
                var fileCounts = await _repositoryWrapper.Files.GetFileCountsByFolderIdsAsync(
                    childIds,
                    cancellationToken
                );

                var filesInParent = await _repositoryWrapper.Files.GetFilesByFolderIdAsync(
                    parentId ?? Guid.Empty
                );

                var filesInParentDtos = new List<FileDto>();
                foreach (var file in filesInParent)
                {
                    var dto = _mapper.Map<FileDto>(file);

                    PreviewResult preview = await PreviewGenerator.GetOrCreatePreviewAsync(
                        file.Id,
                        file.Content,
                        file.Mime
                    );

                    dto.Preview = preview.PreviewBytes;
                    dto.PreviewMime = preview.PreviewMime;
                    dto.PreviewKind = preview.Kind;

                    filesInParentDtos.Add(dto);
                }

                var childDtos = _mapper.Map<List<FolderDto>>(childFolders);

                foreach (var dto in childDtos)
                {
                    dto.HasChildren =
                        subfolderCounts.ContainsKey(dto.Id) || fileCounts.ContainsKey(dto.Id);
                }

                var parentDto = new FolderDto
                {
                    Id = request.FolderId,
                    Name = parentFolder?.Name ?? "Root",
                    ParentFolderId = parentFolder?.ParentFolderId,
                    HasChildren = childDtos.Any() || filesInParentDtos.Any(),
                    SubFolders = childDtos,
                    Files = filesInParentDtos,
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
