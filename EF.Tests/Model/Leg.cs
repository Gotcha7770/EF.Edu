using System;

namespace EF.Tests.Model;

public class Leg
{
    public int Id { get; set; }
    public string StartCode { get; set; }
    public string EndCode { get; set; }
    public DateOnly DepartureDate { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public DateOnly ArrivalDate { get; set; }
    public Trip Trip { get; set; }
    
    public int TripId { get; set; }
}