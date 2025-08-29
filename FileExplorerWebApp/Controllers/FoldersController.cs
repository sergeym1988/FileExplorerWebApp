using FileExplorerWebApp.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static FileExplorerWebApp.Application.Mediator.Commands.FolderCommands;
using static FileExplorerWebApp.Application.Mediator.Queries.FolderQueries;

namespace FileExplorerWebApp.Controllers
{
    /// <summary>
    /// Controller responsible for folder's operations.
    /// </summary>
    [ApiController]
    [Route("api/folders")]
    [EnableRateLimiting("sliding")]
    public class FoldersController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FoldersController"/> class.
        /// </summary>
        public FoldersController(ILogger<FoldersController> logger, IMediator mediator)
            : base(logger, mediator) { }

        /// <summary>
        /// Get root folders.
        /// </summary>
        [HttpGet("root")]
        public async Task<IActionResult> GetRootFolders()
        {
            try
            {
                var folders = await _mediator.Send(new GetRootFoldersQuery());
                return Ok(folders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving folders");
            }

            return BadRequest();
        }

        /// <summary>
        /// Get folder content by id.
        /// </summary>
        [HttpGet("{id}/content")]
        public async Task<IActionResult> GetFolderContentById(Guid id)
        {
            try
            {
                var content = await _mediator.Send(new GetFolderContentByIdQuery(id));
                if (content != null)
                    return Ok(content);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving folder's {FolderId} content ", id);
            }

            return BadRequest();
        }

        /// <summary>
        /// Get subfolders by parent folder id.
        /// </summary>
        [HttpGet("{id}/subfolders")]
        public async Task<IActionResult> GetSubfoldersById(Guid id)
        {
            try
            {
                var folder = await _mediator.Send(new GetSubfoldersByIdQuery(id));
                if (folder != null)
                    return Ok(folder);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving subfolders by folder {FolderId}", id);
            }

            return BadRequest();
        }

        /// <summary>
        /// Create folder.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] FolderDto folderDto)
        {
            try
            {
                var isCreated = await _mediator.Send(new CreateFolderCommand(folderDto));
                if (isCreated)
                    return Created(string.Empty, folderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating folder");
            }

            return BadRequest();
        }

        ///// <summary>
        ///// Renamr folder.
        ///// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> RenameFolder(Guid id, [FromBody] FolderDto folderDto)
        {
            try
            {
                var isUpdated = await _mediator.Send(new RenameFolderCommand(folderDto));
                if (isUpdated)
                    return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while renaming folder {FolderId}", id);
            }

            return BadRequest();
        }

        /// <summary>
        /// Delete folder.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(Guid id)
        {
            try
            {
                var isDeleted = await _mediator.Send(new DeleteFolderCommand(id));
                if (isDeleted)
                    return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting folder {FolderId}", id);
            }

            return BadRequest();
        }
    }
}
