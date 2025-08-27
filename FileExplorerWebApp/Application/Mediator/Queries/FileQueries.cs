using FileExplorerWebApp.Application.DTOs;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Queries
{
    public class FileQueries
    {
        /// <summary>
        /// Get file by Id query record.
        /// </summary>
        public record GetFileByIdQuery(Guid FileId) : IRequest<FileDto?>;

        /// <summary>
        /// Get files by folder Id query record.
        /// </summary>
        public record GetFilesByFolderQuery(Guid FolderId) : IRequest<List<FileDto>>;
    }
}
