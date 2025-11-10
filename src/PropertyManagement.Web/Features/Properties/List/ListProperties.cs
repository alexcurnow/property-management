using MediatR;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Web.Features.MaintenanceRequests.Submit.Models;
using PropertyManagement.Web.Infrastructure.Persistence;

namespace PropertyManagement.Web.Features.Properties.List;

// ============================================================================
// QUERY - Read-only operation
// ============================================================================

public record ListPropertiesQuery : IRequest<ListPropertiesResponse>;

// ============================================================================
// RESPONSE
// ============================================================================

public record ListPropertiesResponse
{
    public List<PropertyDto> Properties { get; init; } = new();
}

public record PropertyDto(
    Guid Id,
    string Name,
    string Address,
    int UnitCount);

public record UnitDto(
    Guid Id,
    string UnitNumber,
    Guid PropertyId);

// ============================================================================
// HANDLER - Query slice (read-only)
// ============================================================================

public class ListPropertiesHandler : IRequestHandler<ListPropertiesQuery, ListPropertiesResponse>
{
    private readonly PropertyManagementDbContext _context;

    public ListPropertiesHandler(PropertyManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ListPropertiesResponse> Handle(
        ListPropertiesQuery request,
        CancellationToken cancellationToken)
    {
        // Query optimized for listing - no complex domain logic
        // Using the Property model from Submit slice (shared tables)
        var properties = await _context.Set<Property>()
            .OrderBy(p => p.Name)
            .Select(p => new PropertyDto(
                p.Id,
                p.Name,
                "", // Address would come from a more complete model if needed
                0   // UnitCount would require a join if needed
            ))
            .ToListAsync(cancellationToken);

        return new ListPropertiesResponse
        {
            Properties = properties
        };
    }
}

// Helper query for units
public record ListUnitsQuery(Guid PropertyId) : IRequest<ListUnitsResponse>;

public record ListUnitsResponse
{
    public List<UnitDto> Units { get; init; } = new();
}

public class ListUnitsHandler : IRequestHandler<ListUnitsQuery, ListUnitsResponse>
{
    private readonly PropertyManagementDbContext _context;

    public ListUnitsHandler(PropertyManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ListUnitsResponse> Handle(
        ListUnitsQuery request,
        CancellationToken cancellationToken)
    {
        var units = await _context.Set<PropertyManagement.Web.Features.MaintenanceRequests.Submit.Models.Unit>()
            .Where(u => u.PropertyId == request.PropertyId)
            .OrderBy(u => u.UnitNumber)
            .Select(u => new UnitDto(u.Id, u.UnitNumber, u.PropertyId))
            .ToListAsync(cancellationToken);

        return new ListUnitsResponse
        {
            Units = units
        };
    }
}
