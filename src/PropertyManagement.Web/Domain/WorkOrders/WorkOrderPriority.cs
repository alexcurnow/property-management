namespace PropertyManagement.Web.Domain.WorkOrders;

/// <summary>
/// Value object representing work order priority levels
/// </summary>
public sealed class WorkOrderPriority : IEquatable<WorkOrderPriority>
{
    public static readonly WorkOrderPriority Low = new(1, nameof(Low));
    public static readonly WorkOrderPriority Medium = new(2, nameof(Medium));
    public static readonly WorkOrderPriority High = new(3, nameof(High));
    public static readonly WorkOrderPriority Emergency = new(4, nameof(Emergency));

    public int Level { get; }
    public string Name { get; }

    private WorkOrderPriority(int level, string name)
    {
        Level = level;
        Name = name;
    }

    public bool IsEmergency() => Level == Emergency.Level;

    public bool IsHigherThan(WorkOrderPriority other) => Level > other.Level;

    public static WorkOrderPriority FromString(string value)
    {
        return value switch
        {
            nameof(Low) => Low,
            nameof(Medium) => Medium,
            nameof(High) => High,
            nameof(Emergency) => Emergency,
            _ => throw new ArgumentException($"Invalid priority: {value}")
        };
    }

    public bool Equals(WorkOrderPriority? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Level == other.Level && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return obj is WorkOrderPriority other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Level, Name);
    }

    public static bool operator ==(WorkOrderPriority? left, WorkOrderPriority? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(WorkOrderPriority? left, WorkOrderPriority? right)
    {
        return !Equals(left, right);
    }

    public override string ToString() => Name;
}
