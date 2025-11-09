using PropertyManagement.Web.Domain.Vendors;

namespace PropertyManagement.Web.Domain.Scheduling;

/// <summary>
/// Aggregate root for preventive maintenance scheduling
/// </summary>
public class MaintenanceSchedule
{
    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid? UnitId { get; private set; }
    public string TaskDescription { get; private set; }
    public RecurrencePattern Recurrence { get; private set; }
    public VendorSpecialization RequiredSpecialization { get; private set; }
    public Guid? PreferredVendorId { get; private set; }
    public DateTime NextScheduledDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor - initializes non-nullable properties to satisfy compiler
    private MaintenanceSchedule()
    {
        TaskDescription = null!;
    }

    private MaintenanceSchedule(
        Guid propertyId,
        Guid? unitId,
        string taskDescription,
        RecurrencePattern recurrence,
        VendorSpecialization requiredSpecialization,
        DateTime nextScheduledDate)
    {
        Id = Guid.NewGuid();
        PropertyId = propertyId;
        UnitId = unitId;
        TaskDescription = taskDescription ?? throw new ArgumentNullException(nameof(taskDescription));
        Recurrence = recurrence;
        RequiredSpecialization = requiredSpecialization;
        NextScheduledDate = nextScheduledDate;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static MaintenanceSchedule Create(
        Guid propertyId,
        Guid? unitId,
        string taskDescription,
        RecurrencePattern recurrence,
        VendorSpecialization requiredSpecialization,
        DateTime nextScheduledDate)
    {
        return new MaintenanceSchedule(propertyId, unitId, taskDescription, recurrence, requiredSpecialization, nextScheduledDate);
    }

    public void SetPreferredVendor(Guid vendorId)
    {
        PreferredVendorId = vendorId;
    }

    public void UpdateNextScheduledDate()
    {
        NextScheduledDate = Recurrence switch
        {
            RecurrencePattern.Daily => NextScheduledDate.AddDays(1),
            RecurrencePattern.Weekly => NextScheduledDate.AddDays(7),
            RecurrencePattern.Monthly => NextScheduledDate.AddMonths(1),
            RecurrencePattern.Quarterly => NextScheduledDate.AddMonths(3),
            RecurrencePattern.Annually => NextScheduledDate.AddYears(1),
            _ => throw new InvalidOperationException($"Unknown recurrence pattern: {Recurrence}")
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool IsDue()
    {
        return IsActive && NextScheduledDate <= DateTime.UtcNow;
    }
}

public enum RecurrencePattern
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Annually
}
