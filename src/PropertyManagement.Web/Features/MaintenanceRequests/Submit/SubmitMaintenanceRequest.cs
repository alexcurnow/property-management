using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Web.Domain.WorkOrders;
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

        // TODO: Add additional validation rules
        // - Validate property exists in database
        // - Validate unit exists if UnitId is provided
        // - Business hours validation for non-emergency requests
    }

    private bool BeValidUrgencyLevel(string urgencyLevel)
    {
        var validLevels = new[] { "Low", "Medium", "High", "Emergency" };
        return validLevels.Contains(urgencyLevel, StringComparer.OrdinalIgnoreCase);
    }
}

// ============================================================================
// HANDLER
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
            // Verify property exists
            var propertyExists = await _context.Properties
                .AnyAsync(p => p.Id == request.PropertyId, cancellationToken);

            if (!propertyExists)
            {
                return SubmitMaintenanceRequestResponse.FailureResult(
                    "Property not found");
            }

            // Verify unit exists if specified
            if (request.UnitId.HasValue)
            {
                var unitExists = await _context.Units
                    .AnyAsync(u => u.Id == request.UnitId.Value && u.PropertyId == request.PropertyId,
                        cancellationToken);

                if (!unitExists)
                {
                    return SubmitMaintenanceRequestResponse.FailureResult(
                        "Unit not found for the specified property");
                }
            }

            // Convert urgency level to priority
            // This should never hit the default case due to FluentValidation,
            // but we throw to catch any programming errors
            var priority = request.UrgencyLevel switch
            {
                "Low" => WorkOrderPriority.Low,
                "Medium" => WorkOrderPriority.Medium,
                "High" => WorkOrderPriority.High,
                "Emergency" => WorkOrderPriority.Emergency,
                _ => throw new InvalidOperationException(
                    $"Invalid urgency level: {request.UrgencyLevel}. This should have been caught by validation.")
            };

            // Create work order aggregate using domain logic
            var workOrder = WorkOrder.Create(
                request.PropertyId,
                request.UnitId,
                request.Description,
                priority);

            // Domain rule: Emergency work orders bypass normal scheduling
            if (workOrder.RequiresImmediateAttention())
            {
                _logger.LogWarning(
                    "Emergency work order created: {WorkOrderId} for property {PropertyId}",
                    workOrder.Id,
                    request.PropertyId);
            }

            // Persist to database
            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Work order {WorkOrderId} created successfully",
                workOrder.Id);

            // TODO: Add domain event publishing here
            // - Publish WorkOrderCreatedEvent
            // - Trigger notifications (email, SMS)
            // - Update real-time dashboard

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
