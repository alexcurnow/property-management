namespace PropertyManagement.Web.Domain.WorkOrders;

/// <summary>
/// Value object representing the status of a work order with valid transitions
/// </summary>
public sealed class WorkOrderStatus : IEquatable<WorkOrderStatus>
{
    public static readonly WorkOrderStatus New = new(nameof(New));
    public static readonly WorkOrderStatus Assigned = new(nameof(Assigned));
    public static readonly WorkOrderStatus InProgress = new(nameof(InProgress));
    public static readonly WorkOrderStatus Completed = new(nameof(Completed));
    public static readonly WorkOrderStatus Cancelled = new(nameof(Cancelled));

    public string Value { get; }

    private WorkOrderStatus(string value)
    {
        Value = value;
    }

    public bool CanTransitionTo(WorkOrderStatus newStatus)
    {
        return Value switch
        {
            nameof(New) => newStatus.Value == Assigned.Value || newStatus.Value == Cancelled.Value,
            nameof(Assigned) => newStatus.Value == InProgress.Value || newStatus.Value == New.Value || newStatus.Value == Cancelled.Value,
            nameof(InProgress) => newStatus.Value == Completed.Value || newStatus.Value == Assigned.Value,
            nameof(Completed) => false,
            nameof(Cancelled) => false,
            _ => false
        };
    }

    public static WorkOrderStatus FromString(string value)
    {
        return value switch
        {
            nameof(New) => New,
            nameof(Assigned) => Assigned,
            nameof(InProgress) => InProgress,
            nameof(Completed) => Completed,
            nameof(Cancelled) => Cancelled,
            _ => throw new ArgumentException($"Invalid work order status: {value}")
        };
    }

    public bool Equals(WorkOrderStatus? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is WorkOrderStatus other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(WorkOrderStatus? left, WorkOrderStatus? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(WorkOrderStatus? left, WorkOrderStatus? right)
    {
        return !Equals(left, right);
    }

    public override string ToString() => Value;
}
