using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class GetSubFoldersHandler
        : IRequestHandler<FolderQueries.GetSubFoldersQuery, List<FolderDto>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public GetSubFoldersHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<List<FolderDto>> Handle(
            FolderQueries.GetSubFoldersQuery request,
            CancellationToken cancellationToken
        )
        {
            var folders = await _repositoryWrapper
                .Folders.FindByCondition(f => f.ParentFolderId == request.ParentFolderId)
                .ToListAsync(cancellationToken);

            return folders
                .Select(f => new FolderDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    ParentFolderId = f.ParentFolderId,
                })
                .ToList();
        }
    }
}
