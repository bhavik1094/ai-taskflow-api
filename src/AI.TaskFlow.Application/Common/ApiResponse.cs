namespace AI.TaskFlow.Application.Common;

public sealed class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IReadOnlyCollection<string> Errors { get; set; } = Array.Empty<string>();

    public static ApiResponse<T> Success(T? data, string message = "")
    {
        return new ApiResponse<T>
        {
            Succeeded = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(string message, params string[] errors)
    {
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = message,
            Errors = errors
        };
    }
}
