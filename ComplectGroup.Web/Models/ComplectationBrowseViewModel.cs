using ComplectGroup.Application.DTOs;

public class ComplectationBrowseViewModel
{
    public List<ComplectationDto> Complectations { get; set; } = new();
    public ComplectationDto? SelectedComplectation { get; set; }
}
