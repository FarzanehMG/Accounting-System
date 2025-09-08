namespace Domain.Common;

public abstract class BaseEntity
{
	public Guid Id { get; init; } = Guid.NewGuid();
	public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
	public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
}