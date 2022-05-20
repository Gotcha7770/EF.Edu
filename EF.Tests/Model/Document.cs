using System;

namespace EF.Tests.Model;

public class Document
{
    public int Id { get; init; }
    
    public int ItemId { get; init; }
    
    public Guid? Signature { get; set; }
    
    public string Test { get; set; }
    
    public Item Item { get; set; }
}