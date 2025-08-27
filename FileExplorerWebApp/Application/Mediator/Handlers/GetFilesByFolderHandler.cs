using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class GetFilesByFolderHandler
        : IRequestHandler<FileQueries.GetFilesByFolderQuery, List<FileDto>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public GetFilesByFolderHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<List<FileDto>> Handle(
            FileQueries.GetFilesByFolderQuery request,
            CancellationToken cancellationToken
        )
        {
            var files = await _repositoryWrapper
                .Files.FindByCondition(f => f.FolderId == request.FolderId)
                .ToListAsync(cancellationToken);

            return files
                .Select(f => new FileDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Size = f.Size,
                    FolderId = f.FolderId,
                    Mime = f.Mime,
                })
                .ToList();
        }
    }
}
