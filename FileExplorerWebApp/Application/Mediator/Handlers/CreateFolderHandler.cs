using AutoMapper;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Domain.Entities;
using MediatR;
using static FileExplorerWebApp.Application.Mediator.Commands.FolderCommands;

namespace FileExplorerWebApp.Application.Mediator.Handlers
{
    public class CreateFolderHandler : IRequestHandler<CreateFolderCommand, bool>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public CreateFolderHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<bool> Handle(
            CreateFolderCommand request,
            CancellationToken cancellationToken
        )
        {
            var folder = _mapper.Map<Folder>(request.FolderDto);
            folder.ParentFolderId =
                folder.ParentFolderId == Guid.Empty ? null : folder.ParentFolderId;

            await _repositoryWrapper.Folders.CreateAsync(folder);

            try
            {
                await _repositoryWrapper.SaveAsync();
                request.FolderDto.Id = folder.Id;
            }
            catch (Exception ex) { }

            return true;
        }
    }
}
