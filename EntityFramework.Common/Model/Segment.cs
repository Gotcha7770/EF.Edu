using System.ComponentModel.DataAnnotations;

namespace EntityFramework.Common.Model;

public class Segment : IEquatable<Segment>
{
    [StringLength(3)]
    public required string StartCode { get; init; }
    public required DateOnly DepartureDate { get; init; }
    [StringLength(3)]
    public required string EndCode { get; init; }
    public required DateOnly ArrivalDate { get; init; }
    [StringLength(2)]
    public required string Carrier { get; init; }
    [StringLength(4)]
    public required string FlightNumber { get; init; }
    
    public string SegmentKey => $"{DepartureDate}-{Carrier}-{FlightNumber}";
    
    public bool Equals(Segment? other)
    {
        return other is not null 
               && other.DepartureDate == DepartureDate
               && other.Carrier == Carrier
               && other.FlightNumber == FlightNumber;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Segment);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DepartureDate, Carrier, FlightNumber);
    }
}