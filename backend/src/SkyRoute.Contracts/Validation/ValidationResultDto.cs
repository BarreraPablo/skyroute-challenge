namespace SkyRoute.Contracts.Validation;

public class ValidationResultDto
{
    public int StatusCode { get; set; } = 200;

    public List<ValidationDto> Conditions { get; set; } = [];
}