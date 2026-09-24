namespace GarmentsERP.Shared.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = [];

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string error)
        => new() { Success = false, Errors = [error], Message = error };

    public static ApiResponse<T> Fail(IReadOnlyList<string> errors)
        => new() { Success = false, Errors = errors, Message = errors.FirstOrDefault() };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkEmpty(string? message = null)
        => new() { Success = true, Message = message };
}
