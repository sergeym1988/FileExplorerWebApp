using AutoMapper;
using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.DTOs.Preview;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Application.Options;
using FileExplorerWebApp.Infrastructure.Utils;
using MediatR;
using Microsoft.Extensions.Options;
using static FileExplorerWebApp.Application.Mediator.Commands.FileCommands;

/// <summary>
/// The upload files handler.
/// </summary>
public class UploadFilesHandler : IRequestHandler<UploadFilesCommand, List<FileDto>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly ILogger<UploadFilesHandler> _logger;
    private readonly FileUploadOptions _options;
    private readonly HashSet<string> _allowedExtensions;
    private readonly HashSet<string> _allowedMimeTypes;
    private readonly long _maxFileSizeBytes;

    public UploadFilesHandler(
        IRepositoryWrapper repositoryWrapper,
        IMapper mapper,
        ILogger<UploadFilesHandler> logger,
        IOptions<FileUploadOptions> options
    )
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
        _options = options.Value ?? new FileUploadOptions();

        _allowedExtensions = new HashSet<string>(
            (_options.AllowedExtensions ?? Array.Empty<string>()).Select(e =>
                e.Trim().ToLowerInvariant()
            ),
            StringComparer.OrdinalIgnoreCase
        );

        _allowedMimeTypes = new HashSet<string>(
            (_options.AllowedMimeTypes ?? Array.Empty<string>()).Select(m =>
                m.Trim().ToLowerInvariant()
            ),
            StringComparer.OrdinalIgnoreCase
        );

        _maxFileSizeBytes = Math.Max(0, _options.MaxFileSizeMb) * 1024L * 1024L;
    }

    public async Task<List<FileDto>> Handle(
        UploadFilesCommand request,
        CancellationToken cancellationToken
    )
    {
        if (request.Files == null || request.Files.Count == 0)
            return new List<FileDto>();

        var incomingFiles = request.Files;
        if (incomingFiles.Count > _options.MaxFilesCount)
        {
            incomingFiles = incomingFiles.Take(_options.MaxFilesCount).ToList();
        }

        if (request.ParentId != Guid.Empty)
        {
            var parent = await _repositoryWrapper.Folders.FindByIdAsync(request.ParentId);
            if (parent == null)
            {
                return new List<FileDto>();
            }
        }

        var createdEntities = new List<FileExplorerWebApp.Domain.Entities.File>();

        foreach (var formFile in incomingFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (formFile == null || formFile.Length == 0)
                continue;

            var fileName = formFile.FileName ?? "file";
            var ext = Path.GetExtension(fileName) ?? string.Empty;
            var mime = (formFile.ContentType ?? string.Empty).Trim().ToLowerInvariant();

            ext = ext.Trim().ToLowerInvariant();
            if (!ext.StartsWith(".") && ext.Length > 0)
                ext = "." + ext;

            if (!string.IsNullOrEmpty(ext) && !_allowedExtensions.Contains(ext))
            {
                continue;
            }

            if (_maxFileSizeBytes > 0 && formFile.Length > _maxFileSizeBytes)
            {
                continue;
            }

            byte[] content;
            using (var ms = new MemoryStream())
            {
                await formFile.CopyToAsync(ms, cancellationToken);
                content = ms.ToArray();
            }

            var entity = new FileExplorerWebApp.Domain.Entities.File
            {
                Id = Guid.NewGuid(),
                Name = Path.GetFileName(fileName),
                Content = content,
                Mime = string.IsNullOrEmpty(mime) ? "application/octet-stream" : mime,
                FolderId = request.ParentId == Guid.Empty ? null : request.ParentId,
                CreatedDateTime = DateTime.UtcNow,
            };

            _repositoryWrapper.Files.Create(entity);
            createdEntities.Add(entity);
        }

        if (createdEntities.Count == 0)
        {
            return new List<FileDto>();
        }

        try
        {
            await _repositoryWrapper.SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while saving uploaded files for parent {ParentId}",
                request.ParentId
            );
            throw;
        }

        var dtos = new List<FileDto>();

        foreach (var file in createdEntities)
        {
            var dto = _mapper.Map<FileDto>(file);

            PreviewResult preview = await PreviewGenerator.GetOrCreatePreviewAsync(
                file.Id,
                file.Content,
                file.Mime
            );

            dto.Preview = preview.PreviewBytes;
            dto.PreviewMime = preview.PreviewMime;
            dto.PreviewKind = preview.Kind;

            dtos.Add(dto);
        }

        return dtos;
    }
}
