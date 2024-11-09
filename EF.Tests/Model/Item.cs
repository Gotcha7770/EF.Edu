namespace EF.Tests.Model
{
    public class Item
    {
        public int Id { get; init; }
        public int Order { get; init; }
        public string Name { get; set; }
        public int Amount { get; set; }
    }
}