namespace Domain.Common;

public abstract class BaseEntity
{
	public required Guid Id { get; init; } = Guid.NewGuid();
	public required bool IsActive { get; set; } = true;
	public required DateTime CreatedDate { get; init; } = DateTime.Now;
	public DateTime? ModifiedDate { get; set; }
}