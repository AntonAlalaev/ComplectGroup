using ComplectGroup.Application.DTOs;

namespace ComplectGroup.Web.Models;

public class PartOperationsIndexViewModel
{
    public List<PartPlanningSummaryDto> Parts { get; set; } = new();

    public List<ChapterDto> Chapters { get; set; } = new();

    public int? ChapterId { get; set; }

    public string? Search { get; set; }

    public int TotalOperations { get; set; }
}
