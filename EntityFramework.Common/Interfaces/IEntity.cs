namespace EntityFramework.Common.Interfaces;

public interface IEntity<out TId>
{
    public TId Id { get; }
}