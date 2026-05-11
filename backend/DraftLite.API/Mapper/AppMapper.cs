using AutoMapper;
using DraftLite.Data.Entities;
using DraftLite.DTO;
using DraftLite.Service.Interfaces;

namespace DraftLite.Api.Mapping;

public sealed class AppMapper : IAppMapper
{
    private readonly IMapper _mapper;

    public AppMapper(IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        return _mapper.Map<TSource, TDestination>(source);
    }
}

public sealed class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        // User -> UserDto
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

        // Project -> ProjectDto
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.OwnerPseudo, opt => opt.MapFrom(src => src.Owner.Pseudo));
    }
}

