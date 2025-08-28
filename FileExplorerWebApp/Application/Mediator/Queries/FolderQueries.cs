using FileExplorerWebApp.Application.DTOs;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Queries
{
    public class FolderQueries
    {
        /// <summary>
        /// Get folder by Id query record.
        /// </summary>
        public record GetFolderContentByIdQuery(Guid FolderId) : IRequest<List<FolderDto>>;

        /// <summary>
        /// Get subfolders by parent folder id.
        /// </summary>
        public record GetSubfoldersByIdQuery(Guid FolderId) : IRequest<List<FolderDto>>;

        /// <summary>
        /// Get all root folders query record.
        /// </summary>
        public record GetRootFoldersQuery() : IRequest<List<FolderDto>>;

        /// <summary>
        /// Get children folders of a folder query record.
        /// </summary>
        public record GetSubFoldersQuery(Guid ParentFolderId) : IRequest<List<FolderDto>>;
    }
}
