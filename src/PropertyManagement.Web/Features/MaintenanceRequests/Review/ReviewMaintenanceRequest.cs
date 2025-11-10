using MediatR;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Web.Features.MaintenanceRequests.Submit.Models;
using PropertyManagement.Web.Infrastructure.Persistence;

namespace PropertyManagement.Web.Features.MaintenanceRequests.Review;

// ============================================================================
// QUERY - Read model for reviewing maintenance requests
// ============================================================================

public record ReviewMaintenanceRequestsQuery : IRequest<ReviewMaintenanceRequestsResponse>;

// ============================================================================
// RESPONSE
// ============================================================================

public record ReviewMaintenanceRequestsResponse
{
    public List<WorkOrderReviewDto> WorkOrders { get; init; } = new();
}

public record WorkOrderReviewDto(
    Guid Id,
    string Description,
    string Status,
    string Priority,
    DateTime CreatedAt,
    string PropertyName);

// ============================================================================
// HANDLER
// ============================================================================

public class ReviewMaintenanceRequestsHandler
    : IRequestHandler<ReviewMaintenanceRequestsQuery, ReviewMaintenanceRequestsResponse>
{
    private readonly PropertyManagementDbContext _context;

    public ReviewMaintenanceRequestsHandler(PropertyManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewMaintenanceRequestsResponse> Handle(
        ReviewMaintenanceRequestsQuery request,
        CancellationToken cancellationToken)
    {
        // Read-optimized query - just get what we need for display
        var workOrders = await _context.Set<WorkOrder>()
            .Where(w => w.Status == "New")
            .OrderByDescending(w => w.PriorityLevel)
            .ThenBy(w => w.CreatedAt)
            .Select(w => new WorkOrderReviewDto(
                w.Id,
                w.Description,
                w.Status,
                w.PriorityName,
                w.CreatedAt,
                w.Property != null ? w.Property.Name : "Unknown"
            ))
            .Take(50)
            .ToListAsync(cancellationToken);

        return new ReviewMaintenanceRequestsResponse
        {
            WorkOrders = workOrders
        };
    }
}
