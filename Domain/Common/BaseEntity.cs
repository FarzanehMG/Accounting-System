namespace Domain.Common;

public interface IEntity
{
    DateTime CreatedDate { get; set; }
    DateTime? ModifiedDate { get; set; }
}

public abstract class BaseEntity<TKey> : IEntity
{
    public TKey Id { get; protected set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
