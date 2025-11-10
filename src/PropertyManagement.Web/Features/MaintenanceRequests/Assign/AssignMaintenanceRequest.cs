using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Web.Features.MaintenanceRequests.Assign.Models;
using PropertyManagement.Web.Infrastructure.Persistence;

namespace PropertyManagement.Web.Features.MaintenanceRequests.Assign;

// ============================================================================
// COMMAND
// ============================================================================

public record AssignMaintenanceRequestCommand : IRequest<AssignMaintenanceRequestResponse>
{
    public Guid WorkOrderId { get; init; }
    public Guid VendorId { get; init; }
    public decimal? EstimatedCost { get; init; }
}

// ============================================================================
// RESPONSE
// ============================================================================

public record AssignMaintenanceRequestResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = new();

    public static AssignMaintenanceRequestResponse SuccessResult(Guid workOrderId, Guid vendorId)
        => new()
        {
            Success = true,
            Message = $"Work order {workOrderId} assigned to vendor successfully"
        };

    public static AssignMaintenanceRequestResponse FailureResult(string error)
        => new()
        {
            Success = false,
            Message = "Failed to assign work order",
            Errors = new List<string> { error }
        };

    public static AssignMaintenanceRequestResponse ValidationFailure(List<string> errors)
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

public class AssignMaintenanceRequestValidator : AbstractValidator<AssignMaintenanceRequestCommand>
{
    public AssignMaintenanceRequestValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty()
            .WithMessage("Work Order ID is required");

        RuleFor(x => x.VendorId)
            .NotEmpty()
            .WithMessage("Vendor ID is required");

        RuleFor(x => x.EstimatedCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.EstimatedCost.HasValue)
            .WithMessage("Estimated cost must be greater than or equal to 0");
    }
}

// ============================================================================
// HANDLER - This is the heart of the vertical slice
// ============================================================================

public class AssignMaintenanceRequestHandler
    : IRequestHandler<AssignMaintenanceRequestCommand, AssignMaintenanceRequestResponse>
{
    private readonly PropertyManagementDbContext _context;
    private readonly ILogger<AssignMaintenanceRequestHandler> _logger;

    public AssignMaintenanceRequestHandler(
        PropertyManagementDbContext context,
        ILogger<AssignMaintenanceRequestHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AssignMaintenanceRequestResponse> Handle(
        AssignMaintenanceRequestCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify work order exists and is not already assigned
            var workOrder = await _context.Set<WorkOrder>()
                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

            if (workOrder == null)
            {
                return AssignMaintenanceRequestResponse.FailureResult(
                    "Work order not found");
            }

            if (workOrder.AssignedVendorId.HasValue)
            {
                return AssignMaintenanceRequestResponse.FailureResult(
                    $"Work order is already assigned to vendor {workOrder.AssignedVendorId}");
            }

            // Verify vendor exists and is active
            var vendor = await _context.Set<Vendor>()
                .FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);

            if (vendor == null)
            {
                return AssignMaintenanceRequestResponse.FailureResult(
                    "Vendor not found");
            }

            if (!vendor.IsActive)
            {
                return AssignMaintenanceRequestResponse.FailureResult(
                    $"Vendor '{vendor.Name}' is not active");
            }

            // Business rule: Assign work order
            workOrder.AssignedVendorId = request.VendorId;
            workOrder.AssignedAt = DateTime.UtcNow;
            workOrder.Status = "Assigned";
            workOrder.UpdatedAt = DateTime.UtcNow;

            if (request.EstimatedCost.HasValue)
            {
                workOrder.EstimatedCostAmount = request.EstimatedCost.Value;
            }

            // Persist changes
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Work order {WorkOrderId} assigned to vendor {VendorId} ({VendorName})",
                workOrder.Id,
                vendor.Id,
                vendor.Name);

            return AssignMaintenanceRequestResponse.SuccessResult(workOrder.Id, vendor.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning work order {WorkOrderId}", request.WorkOrderId);
            return AssignMaintenanceRequestResponse.FailureResult(
                "An unexpected error occurred while assigning the work order");
        }
    }
}
