namespace SkyRoute.Contracts.Validation;

public class ValidationDto
{
    public ValidationSeverity Severity { get; set; }

    public string Message { get; set; } = string.Empty;
}