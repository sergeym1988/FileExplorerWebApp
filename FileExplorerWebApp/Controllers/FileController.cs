using FileExplorerWebApp.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static FileExplorerWebApp.Application.Mediator.Commands.FileCommands;
using static FileExplorerWebApp.Application.Mediator.Queries.FileQueries;

namespace FileExplorerWebApp.Controllers
{
    /// <summary>
    /// Controller responsible for file operations.
    /// </summary>
    [ApiController]
    [Route("api/file")]
    [EnableRateLimiting("sliding")]
    public class FileController : BaseController
    {
        public FileController(ILogger<FileController> logger, IMediator mediator)
            : base(logger, mediator) { }

        /// <summary>
        /// Get list of files inside a folder.
        /// </summary>
        [HttpGet("folder/{folderId:guid}")]
        public async Task<IActionResult> GetFilesByFolder(Guid folderId)
        {
            try
            {
                var files = await _mediator.Send(new GetFilesByFolderQuery(folderId));
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while retrieving files for folder {FolderId}",
                    folderId
                );
            }

            return BadRequest();
        }

        /// <summary>
        /// Get file metadata by id.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFileById(Guid id)
        {
            try
            {
                var file = await _mediator.Send(new GetFileByIdQuery(id));
                if (file != null)
                    return Ok(file);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving file {FileId}", id);
            }

            return BadRequest();
        }

        /// <summary>
        /// Upload one or more files to a folder.
        /// Expects multipart/form-data with 'parentId' field and one or more 'files' parts.
        /// </summary>
        [HttpPost("upload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFiles(
            [FromForm] Guid parentId,
            [FromForm] List<IFormFile> files
        )
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest("No files provided for upload.");

                var result = await _mediator.Send(new UploadFilesCommand(parentId, files));

                if (result != null)
                    return Created(string.Empty, result);

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while uploading files to folder {ParentId}", parentId);
            }

            return BadRequest();
        }

        /// <summary>
        /// Rename (update) file metadata (e.g. name).
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> RenameFile(Guid id, [FromBody] FileDto fileDto)
        {
            try
            {
                if (fileDto == null || fileDto.Id != id)
                    return BadRequest();

                var isUpdated = await _mediator.Send(new RenameFileCommand(fileDto));
                if (isUpdated)
                    return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while renaming file {FileId}", id);
            }

            return BadRequest();
        }

        /// <summary>
        /// Delete file.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteFile(Guid id)
        {
            try
            {
                var isDeleted = await _mediator.Send(new DeleteFileCommand(id));
                if (isDeleted)
                    return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting file {FileId}", id);
            }

            return BadRequest();
        }
    }
}
