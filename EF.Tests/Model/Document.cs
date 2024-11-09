using System;

namespace EF.Tests.Model;

public class Document
{
    public int Id { get; init; }
    public int ItemId { get; init; }
    public Guid? Signature { get; init; }
    public string Test { get; init; }
    
    public Item Item { get; init; }
}