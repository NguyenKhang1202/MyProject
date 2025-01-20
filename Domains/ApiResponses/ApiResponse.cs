using MyProject.Domain.ErrorHandling;

namespace MyProject.Domain.ApiResponses;

public class ApiResponse<T>
{
    public List<ErrorMessage> ErrorMessages { get; set; } = new();
    public T? Data { get; set; }

    public bool IsSuccess => ErrorMessages.Count == 0;

    public static ApiResponse<T> Success(T data) => new() { Data = data };
    public static ApiResponse<T> Fail(ICollection<ErrorMessage> errors) => new() { ErrorMessages = errors.ToList() };
}