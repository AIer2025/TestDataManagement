namespace TestDataManagement.Api.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int? TotalCount { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> SuccessResult(T data, int totalCount, string message = "查询成功")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            TotalCount = totalCount
        };
    }

    public static ApiResponse<T> ErrorResult(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message
        };
    }
}