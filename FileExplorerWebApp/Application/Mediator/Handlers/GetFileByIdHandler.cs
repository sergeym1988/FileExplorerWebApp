using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Mediator.Queries;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class GetFileByIdHandler : IRequestHandler<FileQueries.GetFileByIdQuery, FileDto?>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public GetFileByIdHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<FileDto?> Handle(
            FileQueries.GetFileByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            var file = await _repositoryWrapper.Files.FindByIdAsync(request.FileId);
            if (file == null)
                return null;

            return new FileDto
            {
                Id = file.Id,
                Name = file.Name,
                //Path = file.Path,
                Size = file.Size,
                FolderId = file.FolderId,
                //MimeType = file.MimeType,
            };
        }
    }
}
