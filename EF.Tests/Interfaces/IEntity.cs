namespace EF.Tests.Interfaces;

public interface IEntity<out TId>
{
    public TId Id { get; }
}