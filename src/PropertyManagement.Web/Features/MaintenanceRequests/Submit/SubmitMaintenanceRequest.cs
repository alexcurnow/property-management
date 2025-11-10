using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Web.Features.MaintenanceRequests.Submit.Models;
using PropertyManagement.Web.Infrastructure.Persistence;

namespace PropertyManagement.Web.Features.MaintenanceRequests.Submit;

// ============================================================================
// COMMAND
// ============================================================================

public record SubmitMaintenanceRequestCommand : IRequest<SubmitMaintenanceRequestResponse>
{
    public Guid PropertyId { get; init; }
    public Guid? UnitId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string UrgencyLevel { get; init; } = string.Empty; // Low, Medium, High, Emergency
}

// ============================================================================
// RESPONSE
// ============================================================================

public record SubmitMaintenanceRequestResponse
{
    public bool Success { get; init; }
    public Guid? WorkOrderId { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = new();

    public static SubmitMaintenanceRequestResponse SuccessResult(Guid workOrderId)
        => new()
        {
            Success = true,
            WorkOrderId = workOrderId,
            Message = "Maintenance request submitted successfully"
        };

    public static SubmitMaintenanceRequestResponse FailureResult(string error)
        => new()
        {
            Success = false,
            Message = "Failed to submit maintenance request",
            Errors = new List<string> { error }
        };

    public static SubmitMaintenanceRequestResponse ValidationFailure(List<string> errors)
        => new()
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors
        };
}

// ============================================================================
// VALIDATOR
// ============================================================================

public class SubmitMaintenanceRequestValidator : AbstractValidator<SubmitMaintenanceRequestCommand>
{
    public SubmitMaintenanceRequestValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .WithMessage("Property ID is required");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MinimumLength(10)
            .WithMessage("Description must be at least 10 characters")
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.UrgencyLevel)
            .NotEmpty()
            .WithMessage("Urgency level is required")
            .Must(BeValidUrgencyLevel)
            .WithMessage("Urgency level must be Low, Medium, High, or Emergency");
    }

    private bool BeValidUrgencyLevel(string urgencyLevel)
    {
        var validLevels = new[] { "Low", "Medium", "High", "Emergency" };
        return validLevels.Contains(urgencyLevel, StringComparer.OrdinalIgnoreCase);
    }
}

// ============================================================================
// HANDLER - This is the heart of the vertical slice
// ============================================================================

public class SubmitMaintenanceRequestHandler
    : IRequestHandler<SubmitMaintenanceRequestCommand, SubmitMaintenanceRequestResponse>
{
    private readonly PropertyManagementDbContext _context;
    private readonly ILogger<SubmitMaintenanceRequestHandler> _logger;

    public SubmitMaintenanceRequestHandler(
        PropertyManagementDbContext context,
        ILogger<SubmitMaintenanceRequestHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SubmitMaintenanceRequestResponse> Handle(
        SubmitMaintenanceRequestCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify property exists using this slice's Property model
            var propertyExists = await _context.Set<Property>()
                .AnyAsync(p => p.Id == request.PropertyId, cancellationToken);

            if (!propertyExists)
            {
                return SubmitMaintenanceRequestResponse.FailureResult(
                    "Property not found");
            }

            // Verify unit exists if specified using this slice's Unit model
            if (request.UnitId.HasValue)
            {
                var unitExists = await _context.Set<Models.Unit>()
                    .AnyAsync(u => u.Id == request.UnitId.Value && u.PropertyId == request.PropertyId,
                        cancellationToken);

                if (!unitExists)
                {
                    return SubmitMaintenanceRequestResponse.FailureResult(
                        "Unit not found for the specified property");
                }
            }

            // Convert urgency level to priority using this slice's logic
            var priority = WorkOrderPriority.FromUrgency(request.UrgencyLevel);

            // Create work order using THIS slice's WorkOrder model
            // No complex domain logic - just create the entity
            var workOrder = new WorkOrder
            {
                Id = Guid.NewGuid(),
                PropertyId = request.PropertyId,
                UnitId = request.UnitId,
                Description = request.Description,
                Status = "New",
                PriorityLevel = priority.Level,
                PriorityName = priority.Name,
                EstimatedCostAmount = 0,
                EstimatedCostCurrency = "USD",
                CreatedAt = DateTime.UtcNow
            };

            // Business rule: Log emergency work orders
            if (priority.Level == 4)
            {
                _logger.LogWarning(
                    "Emergency work order created: {WorkOrderId} for property {PropertyId}",
                    workOrder.Id,
                    request.PropertyId);
            }

            // Persist to database
            _context.Set<WorkOrder>().Add(workOrder);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Work order {WorkOrderId} created successfully",
                workOrder.Id);

            return SubmitMaintenanceRequestResponse.SuccessResult(workOrder.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting maintenance request");
            return SubmitMaintenanceRequestResponse.FailureResult(
                "An unexpected error occurred while submitting the request");
        }
    }
}
