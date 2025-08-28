using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.DTOs.Preview;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using FileExplorerWebApp.Domain.Entities;
using FileExplorerWebApp.Infrastructure.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    /// <summary>
    /// The get folder content by parent id handler.
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;FileExplorerWebApp.Application.Mediator.Queries.FolderQueries.GetFolderContentByIdQuery, System.Collections.Generic.List&lt;FileExplorerWebApp.Application.DTOs.FolderDto&gt;&gt;" />
    public class GetFolderContentByIdHandler
        : IRequestHandler<FolderQueries.GetFolderContentByIdQuery, List<FolderDto>?>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger<GetFolderContentByIdHandler> _logger;

        public GetFolderContentByIdHandler(
            IRepositoryWrapper repositoryWrapper,
            ILogger<GetFolderContentByIdHandler> logger
        )
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<List<FolderDto>?> Handle(
            FolderQueries.GetFolderContentByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            try
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
                    {
                        return new List<FolderDto>();
                    }
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

                var filesInParentEntities = await _repositoryWrapper
                    .Files.FindByCondition(fi => fi.FolderId == parentIdForQuery)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var filesInParentDtos = new List<FileDto>();
                foreach (var f in filesInParentEntities)
                {
                    PreviewResult preview = await PreviewGenerator.GetOrCreatePreviewAsync(
                        f.Id,
                        f.Content,
                        f.Mime
                    );

                    var dto = new FileDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Mime = f.Mime,
                        FolderId = f.FolderId,
                        CreatedDateTime = f.CreatedDateTime,
                        LastModifiedDateTime = f.LastModifiedDateTime,

                        Preview = preview.PreviewBytes,
                        PreviewMime = preview.PreviewMime,
                        PreviewKind = preview.Kind,
                    };

                    filesInParentDtos.Add(dto);
                }

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
                    HasChildren = (childDtos?.Any() == true) || (filesInParentDtos?.Any() == true),
                    SubFolders = childDtos,
                    Files = filesInParentDtos,
                    CreatedDateTime = parentEntity is Audit pa ? pa.CreatedDateTime : null,
                    LastModifiedDateTime = parentEntity is Audit pb
                        ? pb.LastModifiedDateTime
                        : null,
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
