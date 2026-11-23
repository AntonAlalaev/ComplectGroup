using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Web.Models;

public class PartRouteViewModel
{
    public PartDto Part { get; set; } = default!;

    public List<PartOperationDto> Operations { get; set; } = new();

    public decimal TotalPlannedHours { get; set; }
}
