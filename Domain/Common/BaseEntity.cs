namespace Domain.Common;

public abstract class BaseEntity
{
	public Guid Id { get; init; } = Guid.NewGuid();
	public DateTime CreatedDate { get; init; } = DateTime.Now;
	public DateTime? ModifiedDate { get; set; }
}