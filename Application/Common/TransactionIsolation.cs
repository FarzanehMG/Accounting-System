namespace Application.Common;

public enum TransactionIsolation
{
    Default = 0,
    ReadCommitted = 1,
    RepeatableRead = 2,
    Serializable = 3
}