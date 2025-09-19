using System.ComponentModel.DataAnnotations;

namespace EntityFramework.Common.Model;

public class Route
{
    public required Guid Id { get; init; }
    [StringLength(3)]
    public required string StartCode { get; init; }
    public DateOnly DepartureDate { get; init; }
    [StringLength(3)]
    public required string EndCode { get; init; }
    public required List<Segment> Segments { get; init; }
}