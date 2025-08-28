using FileExplorerWebApp.Application.DTOs;
using MediatR;

namespace FileExplorerWebApp.Application.Mediator.Commands
{
    /// <summary>
    /// The file commands.
    /// </summary>
    public class FileCommands
    {
        /// <summary>
        /// Create file command record.
        /// </summary>
        public record UploadFilesCommand(Guid ParentId, List<IFormFile> Files)
            : IRequest<List<FileDto>>;

        /// <summary>
        /// Rename file command record.
        /// </summary>
        public record RenameFileCommand(FileDto FileDto) : IRequest<bool>;

        /// <summary>
        /// Delete file command record.
        /// </summary>
        public record DeleteFileCommand(Guid FileId) : IRequest<bool>;
    }
}
