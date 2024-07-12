using System;

namespace EF.Tests.Model;

public class Point
{
    public int Id { get; set; }
    public string Code { get; set; }
    public DateOnly DepartureDate { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public Trip Trip { get; set; }
}