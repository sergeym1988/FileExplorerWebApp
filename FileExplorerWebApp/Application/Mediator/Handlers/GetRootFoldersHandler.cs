using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
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

                var rootFilesDto = rootFiles
                    .Select(fi => new FileDto
                    {
                        Id = fi.Id,
                        Name = fi.Name,
                        Mime = fi.Mime,
                        FolderId = fi.FolderId,
                        CreatedDateTime = fi.CreatedDateTime,
                        LastModifiedDateTime = fi.LastModifiedDateTime,
                    })
                    .ToList();

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
