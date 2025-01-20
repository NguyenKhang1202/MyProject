using AutoMapper;
using MyProject.Domain;
using MyProject.Domain.Dtos;
using MyProject.Domain.ErrorHandling;
using MyProject.Repos;

namespace MyProject.Services;

public interface IUserService
{
    Task<User?> GetById(int id);
    Task<List<ErrorMessage>> Update(int id, UpdateUserDto updateUserDto);
}

public class UserService(IUserRepo userRepo, IMapper mapper): IUserService
{
    public async Task<User?> GetById(int id)
    {
        var user = await userRepo.GetByIdAsync(id);
        return user;
    }

    public async Task<List<ErrorMessage>> Update(int id, UpdateUserDto updateUserDto)
    {
        var errors = new List<ErrorMessage>();
        var user = await userRepo.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            errors.Add(new ErrorMessage()
            {
                Code = 404,
                Message = "User not found."
            });
        }

        mapper.Map(updateUserDto, user);

        userRepo.Update(user!);
        await userRepo.SaveChangesAsync();

        return errors;
    }
}