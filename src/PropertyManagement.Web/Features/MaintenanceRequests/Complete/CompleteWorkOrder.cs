using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Web.Features.MaintenanceRequests.Complete.Models;
using PropertyManagement.Web.Infrastructure.Persistence;

namespace PropertyManagement.Web.Features.MaintenanceRequests.Complete;

// ============================================================================
// COMMAND
// ============================================================================

public record CompleteWorkOrderCommand : IRequest<CompleteWorkOrderResponse>
{
    public Guid WorkOrderId { get; init; }
    public string CompletionNotes { get; init; } = string.Empty;
    public decimal ActualCost { get; init; }
}

// ============================================================================
// RESPONSE
// ============================================================================

public record CompleteWorkOrderResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = new();

    public static CompleteWorkOrderResponse SuccessResult(Guid workOrderId)
        => new()
        {
            Success = true,
            Message = $"Work order {workOrderId} marked as completed successfully"
        };

    public static CompleteWorkOrderResponse FailureResult(string error)
        => new()
        {
            Success = false,
            Message = "Failed to complete work order",
            Errors = new List<string> { error }
        };

    public static CompleteWorkOrderResponse ValidationFailure(List<string> errors)
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

public class CompleteWorkOrderValidator : AbstractValidator<CompleteWorkOrderCommand>
{
    public CompleteWorkOrderValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty()
            .WithMessage("Work Order ID is required");

        RuleFor(x => x.CompletionNotes)
            .NotEmpty()
            .WithMessage("Completion notes are required")
            .MinimumLength(10)
            .WithMessage("Completion notes must be at least 10 characters")
            .MaximumLength(2000)
            .WithMessage("Completion notes cannot exceed 2000 characters");

        RuleFor(x => x.ActualCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Actual cost must be greater than or equal to 0");
    }
}

// ============================================================================
// HANDLER - This is the heart of the vertical slice
// ============================================================================

public class CompleteWorkOrderHandler
    : IRequestHandler<CompleteWorkOrderCommand, CompleteWorkOrderResponse>
{
    private readonly PropertyManagementDbContext _context;
    private readonly ILogger<CompleteWorkOrderHandler> _logger;

    public CompleteWorkOrderHandler(
        PropertyManagementDbContext context,
        ILogger<CompleteWorkOrderHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CompleteWorkOrderResponse> Handle(
        CompleteWorkOrderCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify work order exists and is assigned
            var workOrder = await _context.Set<WorkOrder>()
                .Include(w => w.Vendor)
                .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

            if (workOrder == null)
            {
                return CompleteWorkOrderResponse.FailureResult(
                    "Work order not found");
            }

            // Business rule: Can only complete assigned work orders
            if (!workOrder.AssignedVendorId.HasValue)
            {
                return CompleteWorkOrderResponse.FailureResult(
                    "Cannot complete work order that has not been assigned to a vendor");
            }

            // Business rule: Cannot complete already completed work orders
            if (workOrder.Status == "Completed")
            {
                return CompleteWorkOrderResponse.FailureResult(
                    "Work order is already completed");
            }

            // Business rule: Mark work order as completed
            workOrder.Status = "Completed";
            workOrder.CompletedAt = DateTime.UtcNow;
            workOrder.CompletionNotes = request.CompletionNotes;
            workOrder.ActualCostAmount = request.ActualCost;
            workOrder.ActualCostCurrency = "USD";
            workOrder.UpdatedAt = DateTime.UtcNow;

            // Business rule: Log if actual cost significantly exceeds estimate
            var costDifference = workOrder.ActualCostAmount - workOrder.EstimatedCostAmount;
            var costDifferencePercentage = workOrder.EstimatedCostAmount > 0
                ? (costDifference / workOrder.EstimatedCostAmount) * 100
                : 0;

            if (costDifferencePercentage > 20)
            {
                _logger.LogWarning(
                    "Work order {WorkOrderId} completed with actual cost ${ActualCost} exceeding estimate ${EstimatedCost} by {Percentage:F2}%",
                    workOrder.Id,
                    workOrder.ActualCostAmount,
                    workOrder.EstimatedCostAmount,
                    costDifferencePercentage);
            }

            // Persist changes
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Work order {WorkOrderId} marked as completed by vendor {VendorName}",
                workOrder.Id,
                workOrder.Vendor?.Name ?? "Unknown");

            return CompleteWorkOrderResponse.SuccessResult(workOrder.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing work order {WorkOrderId}", request.WorkOrderId);
            return CompleteWorkOrderResponse.FailureResult(
                "An unexpected error occurred while completing the work order");
        }
    }
}
