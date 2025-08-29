using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.DTOs.Preview;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using FileExplorerWebApp.Infrastructure.Utils;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    /// <summary>
    /// The get root folders handler.
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;FileExplorerWebApp.Application.Mediator.Queries.FolderQueries.GetRootFoldersQuery, System.Collections.Generic.List&lt;FileExplorerWebApp.Application.DTOs.FolderDto&gt;&gt;" />
    public class GetRootFoldersHandler
        : IRequestHandler<FolderQueries.GetRootFoldersQuery, List<FolderDto>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger<GetRootFoldersHandler> _logger;

        public GetRootFoldersHandler(
            IRepositoryWrapper repositoryWrapper,
            ILogger<GetRootFoldersHandler> logger
        )
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<List<FolderDto>> Handle(
            FolderQueries.GetRootFoldersQuery request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var rootFolders = await _repositoryWrapper.Folders.GetRootFoldersAsync();

                var rootFolderIds = rootFolders.Select(f => f.Id).ToList();

                var subfolderCounts = await _repositoryWrapper.Folders.GetSubfolderCountsAsync(
                    rootFolderIds,
                    cancellationToken
                );
                var fileCounts = await _repositoryWrapper.Files.GetFileCountsByFolderIdsAsync(
                    rootFolderIds,
                    cancellationToken
                );

                var folderDtos = rootFolders
                    .Select(f => new FolderDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        ParentFolderId = f.ParentFolderId,
                        HasChildren =
                            subfolderCounts.ContainsKey(f.Id) || fileCounts.ContainsKey(f.Id),
                        CreatedDateTime = f.CreatedDateTime,
                        LastModifiedDateTime = f.LastModifiedDateTime,
                        SubFolders = null,
                        Files = null,
                    })
                    .ToList();

                var rootFiles = await _repositoryWrapper.Files.GetRootFilesAsync();

                var rootFilesDto = new List<FileDto>();
                foreach (var file in rootFiles)
                {
                    var dto = new FileDto
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Mime = file.Mime,
                        FolderId = file.FolderId,
                        CreatedDateTime = file.CreatedDateTime,
                        LastModifiedDateTime = file.LastModifiedDateTime,
                    };

                    PreviewResult preview = await PreviewGenerator.GetOrCreatePreviewAsync(
                        file.Id,
                        file.Content,
                        file.Mime
                    );

                    dto.Preview = preview.PreviewBytes;
                    dto.PreviewMime = preview.PreviewMime;
                    dto.PreviewKind = preview.Kind;

                    rootFilesDto.Add(dto);
                }

                var root = new FolderDto
                {
                    Id = Guid.Empty,
                    Name = "Root",
                    ParentFolderId = null,
                    HasChildren = folderDtos.Any() || rootFilesDto.Any(),
                    SubFolders = folderDtos,
                    Files = rootFilesDto,
                };

                return new List<FolderDto> { root };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving root folders");
                return new List<FolderDto>();
            }
        }
    }
}
