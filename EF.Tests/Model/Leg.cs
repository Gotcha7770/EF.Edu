using System;

namespace EF.Tests.Model;

public class Leg
{
    public int Id { get; init; }
    public string StartCode { get; init; }
    public string EndCode { get; init; }
    public DateTime DepartureDate { get; init; }
    public DateTime ArrivalDate { get; init; }
    public int TripId { get; init; }
    
    public Trip Trip { get; init; }
}