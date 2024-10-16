using Microsoft.AspNetCore.Http;

namespace KeycloakWebAPI.Dtos;

public class Result<T>
{
    public bool IsSuccess { get; set; } = default!;
    public T? Value { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; } = default!;
    public static Result<T> Success(T value, string? message = null, int statusCode = 200)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Value = value,
            Message = message,
            StatusCode = statusCode
        };
    }
    public static Result<T> Failure(string errorMessage, IEnumerable<string>? errors = null, int statusCode = 400)
    {
        string combinedErrors = errors != null ? string.Join(", ", errors) : string.Empty;
        return new Result<T>
        {
            IsSuccess = false,
            Message = !string.IsNullOrEmpty(combinedErrors) ? $"{errorMessage}, {combinedErrors}" : errorMessage,
            StatusCode = statusCode
        };
    }

    public static Result<T> Exception(Exception ex, string? message = null, int statusCode = 500)
    {
        string combinedMessage = !string.IsNullOrEmpty(message) ? $"{message}, {ex.Message}" : ex.Message;

        return new Result<T>
        {
            IsSuccess = false,
            Message = combinedMessage,
            StatusCode = statusCode
        };
    }
}
