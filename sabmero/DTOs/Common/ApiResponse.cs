namespace sabmero.DTOs.Common;

// A standard envelope so EVERY endpoint returns the same shape:
// { "success": true, "message": "...", "data": { ... } }
// This makes the Flutter side simple — it always parses the same structure.
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message, Data = default };
}

// Used when an endpoint returns a long list (products, orders) split into pages.
// Flutter sends ?page=1&pageSize=20 and uses TotalPages to build infinite scroll.
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}