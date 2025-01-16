using AutoMapper;
using MyProject.Domain;
using MyProject.Domain.Dtos;

namespace MyProject.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UpdateUserDto, User>()
            .ForMember(d => d.Username, s => s.Ignore())
            .ForMember(d => d.Email, s => s.Ignore());
    }
}