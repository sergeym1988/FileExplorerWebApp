using AutoMapper;
using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using MediatR;
using static FileExplorerWebApp.Application.Mediator.Commands.FileCommands;
using File = FileExplorerWebApp.Domain.Entities.File;

public class UploadFilesHandler : IRequestHandler<UploadFilesCommand, List<FileDto>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly ILogger<UploadFilesHandler> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ".txt",
        ".png",
        ".jpg",
        ".jpeg",
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/plain",
        "image/png",
        "image/jpeg",
    };

    public UploadFilesHandler(
        IRepositoryWrapper repositoryWrapper,
        IMapper mapper,
        ILogger<UploadFilesHandler> logger
    )
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<FileDto>> Handle(
        UploadFilesCommand request,
        CancellationToken cancellationToken
    )
    {
        if (request.Files == null || request.Files.Count == 0)
            return new List<FileDto>();

        if (request.ParentId != Guid.Empty)
        {
            var parent = await _repositoryWrapper.Folders.FindByIdAsync(request.ParentId);
            if (parent == null)
            {
                _logger.LogWarning("Parent folder {ParentId} not found", request.ParentId);
                return new List<FileDto>();
            }
        }

        var createdEntities = new List<File>();

        foreach (var formFile in request.Files)
        {
            if (formFile.Length == 0)
                continue;

            var ext = Path.GetExtension(formFile.FileName ?? string.Empty);
            var mime = formFile.ContentType ?? string.Empty;

            if (!string.IsNullOrEmpty(ext) && !AllowedExtensions.Contains(ext))
            {
                _logger.LogWarning(
                    "File {FileName} has not allowed extension {Ext}",
                    formFile.FileName,
                    ext
                );
                continue;
            }

            if (!string.IsNullOrEmpty(mime) && !AllowedMimeTypes.Contains(mime))
            {
                _logger.LogWarning(
                    "File {FileName} has not allowed mime {Mime}",
                    formFile.FileName,
                    mime
                );
            }

            byte[] content;
            using (var ms = new MemoryStream())
            {
                await formFile.CopyToAsync(ms, cancellationToken);
                content = ms.ToArray();
            }

            var entity = new File
            {
                Id = Guid.NewGuid(),
                Name = Path.GetFileName(formFile.FileName ?? "file"),
                Content = content,
                Mime = mime ?? "application/octet-stream",
                Size = formFile.Length,
                FolderId = request.ParentId == Guid.Empty ? null : request.ParentId,
                CreatedDateTime = DateTime.UtcNow,
            };

            _repositoryWrapper.Files.Create(entity);
            createdEntities.Add(entity);
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

        //var dtos = createdEntities.Select(e => new FileDto
        //{
        //    Id = e.Id,
        //    Name = e.Name,
        //    Mime = e.Mime,
        //    Size = e.Size,
        //    FolderId = e.FolderId,
        //    CreatedDateTime = (e as Audit)?.CreatedDateTime ?? null, // если Audit содержит
        //    LastModifiedDateTime = (e as Audit)?.LastModifiedDateTime ?? null
        //}).ToList();

        // Если у тебя есть AutoMapper конфигурация, можно заменить ручной маппинг:
        var dtos = _mapper.Map<List<FileDto>>(createdEntities);

        return dtos;
    }
}
