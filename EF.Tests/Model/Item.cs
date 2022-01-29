namespace EF.Tests.Model
{
    public class Item
    {
        public Item(int id, string name, int amount)
        {
            Id = id;
            Name = name;
            Amount = amount;
        }

        public int Id { get; }

        public string Name { get; set; }

        public int Amount { get; set; }
    }
}