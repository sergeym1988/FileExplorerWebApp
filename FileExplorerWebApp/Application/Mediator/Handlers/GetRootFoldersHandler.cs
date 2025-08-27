using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using FileExplorerWebApp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class GetRootFoldersHandler
        : IRequestHandler<FolderQueries.GetRootFoldersQuery, List<FolderDto>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public GetRootFoldersHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<List<FolderDto>> Handle(
            FolderQueries.GetRootFoldersQuery request,
            CancellationToken cancellationToken
        )
        {
            var rootFolders = await _repositoryWrapper
                .Folders.FindByCondition(f => f.ParentFolderId == null)
                .ToListAsync(cancellationToken);

            var folderDtos = new List<FolderDto>();
            if (rootFolders.Any())
            {
                var rootFolderIds = rootFolders.Select(f => f.Id).ToList();

                var subfolderCounts = await _repositoryWrapper
                    .Folders.FindByCondition(f =>
                        f.ParentFolderId != null && rootFolderIds.Contains(f.ParentFolderId.Value)
                    )
                    .GroupBy(f => f.ParentFolderId)
                    .Select(g => new { ParentId = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                var fileCounts = await _repositoryWrapper
                    .Files.FindByCondition(f =>
                        f.FolderId != null && rootFolderIds.Contains(f.FolderId.Value)
                    )
                    .GroupBy(f => f.FolderId)
                    .Select(g => new { ParentId = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                folderDtos = rootFolders
                    .Select(f => new FolderDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        ParentFolderId = f.ParentFolderId,
                        HasChildren =
                            subfolderCounts.Any(x => x.ParentId == f.Id)
                            || fileCounts.Any(x => x.ParentId == f.Id),
                        CreatedDateTime = (f as Audit)?.CreatedDateTime,
                        LastModifiedDateTime = (f as Audit)?.LastModifiedDateTime,
                    })
                    .ToList();
            }

            var rootFiles = await _repositoryWrapper
                .Files.FindByCondition(fi => fi.FolderId == null)
                .Select(fi => new FileDto
                {
                    Id = fi.Id,
                    Name = fi.Name,
                    Mime = fi.Mime,
                    Size = fi.Size,
                    FolderId = fi.FolderId,
                    CreatedDateTime = fi.CreatedDateTime,
                    LastModifiedDateTime = fi.LastModifiedDateTime,
                })
                .ToListAsync(cancellationToken);

            var root = new FolderDto
            {
                Id = Guid.Empty,
                Name = "Root",
                ParentFolderId = null,
                HasChildren = (folderDtos?.Any() == true) || (rootFiles?.Any() == true),
                SubFolders = folderDtos,
                Files = rootFiles,
            };

            return new List<FolderDto> { root };
        }
    }
}
