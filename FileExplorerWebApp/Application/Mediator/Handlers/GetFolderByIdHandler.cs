using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using FileExplorerWebApp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class GetFolderByIdHandler
        : IRequestHandler<FolderQueries.GetFolderByIdQuery, List<FolderDto>?>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public GetFolderByIdHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<List<FolderDto>?> Handle(
            FolderQueries.GetFolderByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            Guid? parentIdForQuery = request.FolderId == Guid.Empty ? null : request.FolderId;

            Folder? parentEntity = null;
            if (parentIdForQuery.HasValue)
            {
                parentEntity = await _repositoryWrapper
                    .Folders.FindByCondition(f => f.Id == parentIdForQuery.Value)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (parentEntity == null)
                    return new List<FolderDto>();
            }

            var childFolders = await _repositoryWrapper
                .Folders.FindByCondition(f => f.ParentFolderId == parentIdForQuery)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var childIds = childFolders.Select(f => f.Id).ToList();

            var subfolderCounts = new List<(Guid ParentId, int Count)>();
            if (childIds.Count > 0)
            {
                var subfolderGroups = await _repositoryWrapper
                    .Folders.FindByCondition(f =>
                        f.ParentFolderId != null && childIds.Contains(f.ParentFolderId.Value)
                    )
                    .AsNoTracking()
                    .GroupBy(f => f.ParentFolderId)
                    .Select(g => new { ParentId = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                subfolderCounts = subfolderGroups
                    .Where(x => x.ParentId.HasValue)
                    .Select(x => (x.ParentId.Value, x.Count))
                    .ToList();
            }

            var fileCountsForChildren = new List<(Guid ParentId, int Count)>();
            if (childIds.Count > 0)
            {
                var fileGroups = await _repositoryWrapper
                    .Files.FindByCondition(f =>
                        f.FolderId != null && childIds.Contains(f.FolderId.Value)
                    )
                    .AsNoTracking()
                    .GroupBy(f => f.FolderId)
                    .Select(g => new { ParentId = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                fileCountsForChildren = fileGroups
                    .Where(x => x.ParentId.HasValue)
                    .Select(x => (x.ParentId.Value, x.Count))
                    .ToList();
            }

            var filesInParent = await _repositoryWrapper
                .Files.FindByCondition(fi => fi.FolderId == parentIdForQuery)
                .AsNoTracking()
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

            var childDtos = childFolders
                .Select(f => new FolderDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    ParentFolderId = f.ParentFolderId,
                    HasChildren =
                        subfolderCounts.Any(x => x.ParentId == f.Id && x.Count > 0)
                        || fileCountsForChildren.Any(x => x.ParentId == f.Id && x.Count > 0),
                    CreatedDateTime = (f as Audit)?.CreatedDateTime,
                    LastModifiedDateTime = (f as Audit)?.LastModifiedDateTime,
                    SubFolders = null,
                    Files = null,
                })
                .ToList();

            var parentDto = new FolderDto
            {
                Id = request.FolderId,
                Name = parentEntity != null ? parentEntity.Name : "Root",
                ParentFolderId = parentEntity?.ParentFolderId,
                HasChildren = (childDtos?.Any() == true) || (filesInParent?.Any() == true),
                SubFolders = childDtos,
                Files = filesInParent,
                CreatedDateTime = parentEntity is Audit pa ? pa.CreatedDateTime : null,
                LastModifiedDateTime = parentEntity is Audit pb ? pb.LastModifiedDateTime : null,
            };

            return new List<FolderDto> { parentDto };
        }
    }
}
