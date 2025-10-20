namespace GatewayService.Models;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}

public class ErrorDescription
{
    public string Field { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class ValidationErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ErrorDescription> Errors { get; set; } = new();
}