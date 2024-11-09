using System.Collections.Generic;

namespace EF.Tests.Model;

// https://github.com/koenbeuk/EntityFrameworkCore.Projectables?tab=readme-ov-file

public class Trip
{
    public int Id { get; init; }
    public ICollection<Leg> Legs { get; init; } = [];
}