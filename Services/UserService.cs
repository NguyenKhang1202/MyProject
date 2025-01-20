using AutoMapper;
using MyProject.Domain;
using MyProject.Domain.ApiResponses;
using MyProject.Domain.Dtos;
using MyProject.Domain.ErrorHandling;
using MyProject.Repos;

namespace MyProject.Services;

public interface IUserService
{
    Task<ApiResponse<User?>> GetById(int id);
    Task<ApiResponse<User?>> Update(int id, UpdateUserDto updateUserDto);
}

public class UserService(IUserRepo userRepo, IMapper mapper): IUserService
{
    public async Task<ApiResponse<User?>> GetById(int id)
    {
        var user = await userRepo.GetByIdAsync(id);
        return ApiResponse<User?>.Success(user);
    }

    public async Task<ApiResponse<User?>> Update(int id, UpdateUserDto updateUserDto)
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

        if (errors.Any() is false)
        {
            mapper.Map(updateUserDto, user);

            userRepo.Update(user!);
            await userRepo.SaveChangesAsync();
            return ApiResponse<User>.Success(user!);
        }

        return ApiResponse<User?>.Fail(errors);
    }
}