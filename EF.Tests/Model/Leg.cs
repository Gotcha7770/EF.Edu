using System;

namespace EF.Tests.Model;

public class Leg
{
    public int Id { get; set; }
    public string StartCode { get; set; }
    public string EndCode { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public Trip Trip { get; set; }
    
    public int TripId { get; set; }
}