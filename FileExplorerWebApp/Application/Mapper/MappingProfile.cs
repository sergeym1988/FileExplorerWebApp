using AutoMapper;
using FileExplorerWebApp.Application.DTOs;
using FileExplorerWebApp.Domain.Entities;

namespace FileExplorerWebApp.Application.Mapper
{
    /// <summary>
    /// The main mapping profile.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfile"/> class.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<Audit, AuditDto>().ReverseMap();

            CreateMap<Folder, FolderDto>()
                .ReverseMap()
                //.ForMember(dest => dest.ParentFolder, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Domain.Entities.File, FileDto>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
