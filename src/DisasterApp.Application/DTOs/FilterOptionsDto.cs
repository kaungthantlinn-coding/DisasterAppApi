namespace DisasterApp.Application.DTOs;

public class FilterOptionsDto
{
    public List<string> Actions { get; set; } = new();
    public List<string> TargetTypes { get; set; } = new();
}
//