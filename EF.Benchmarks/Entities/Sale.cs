namespace EF.Benchmarks.Entities;

public enum SalesStatus
{
    Pending,
    Completed,
    Cancelled
}

public class Sale
{
    public int Id { get; init; }
    public DateTime SaleDate { get; init; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public SalesStatus Status { get; set; }

    public Person Customer { get; set; }
    public Product Product { get; set; }
}