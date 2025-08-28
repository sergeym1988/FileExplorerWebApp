using FileExplorerWebApp.Application.DTOs;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Commands
{
    /// <summary>
    /// The folder commands.
    /// </summary>
    public class FolderCommands
    {
        /// <summary>
        /// Create folder command record.
        /// </summary>
        public record CreateFolderCommand(FolderDto FolderDto) : IRequest<bool>;

        /// <summary>
        /// Update folder command record.
        /// </summary>
        public record RenameFolderCommand(FolderDto FolderDto) : IRequest<bool>;

        /// <summary>
        /// Delete folder command record.
        /// </summary>
        public record DeleteFolderCommand(Guid FolderId) : IRequest<bool>;
    }
}
