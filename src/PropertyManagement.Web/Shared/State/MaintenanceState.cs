using Fluxor;
using MediatR;
using PropertyManagement.Web.Features.MaintenanceRequests.Submit;

namespace PropertyManagement.Web.Shared.State;

// ============================================================================
// STATE
// ============================================================================

[FeatureState]
public record MaintenanceState
{
    public bool IsSubmitting { get; init; }
    public List<WorkOrderDto> PendingWorkOrders { get; init; } = new();
    public string? LastError { get; init; }
    public Guid? LastSubmittedWorkOrderId { get; init; }
}

public record WorkOrderDto(
    Guid Id,
    string Description,
    string Priority,
    DateTime CreatedAt,
    bool IsOptimistic = false);

// ============================================================================
// ACTIONS
// ============================================================================

public record SubmitMaintenanceRequestAction(
    Guid PropertyId,
    Guid? UnitId,
    string Description,
    string UrgencyLevel);

public record SubmitMaintenanceRequestSuccessAction(Guid WorkOrderId);

public record SubmitMaintenanceRequestFailureAction(string Error);

// ============================================================================
// REDUCERS
// ============================================================================

public static class MaintenanceReducers
{
    [ReducerMethod]
    public static MaintenanceState OnSubmitMaintenanceRequest(
        MaintenanceState state,
        SubmitMaintenanceRequestAction action)
    {
        // Optimistic update: Add work order immediately
        var optimisticWorkOrder = new WorkOrderDto(
            Guid.NewGuid(),
            action.Description,
            action.UrgencyLevel,
            DateTime.UtcNow,
            IsOptimistic: true);

        return state with
        {
            IsSubmitting = true,
            PendingWorkOrders = state.PendingWorkOrders.Append(optimisticWorkOrder).ToList(),
            LastError = null
        };
    }

    [ReducerMethod]
    public static MaintenanceState OnSubmitMaintenanceRequestSuccess(
        MaintenanceState state,
        SubmitMaintenanceRequestSuccessAction action)
    {
        // Remove optimistic work order and add confirmed one
        var pendingWorkOrders = state.PendingWorkOrders
            .Where(wo => !wo.IsOptimistic)
            .ToList();

        return state with
        {
            IsSubmitting = false,
            PendingWorkOrders = pendingWorkOrders,
            LastSubmittedWorkOrderId = action.WorkOrderId,
            LastError = null
        };
    }

    [ReducerMethod]
    public static MaintenanceState OnSubmitMaintenanceRequestFailure(
        MaintenanceState state,
        SubmitMaintenanceRequestFailureAction action)
    {
        // Remove optimistic work order on failure
        var pendingWorkOrders = state.PendingWorkOrders
            .Where(wo => !wo.IsOptimistic)
            .ToList();

        return state with
        {
            IsSubmitting = false,
            PendingWorkOrders = pendingWorkOrders,
            LastError = action.Error
        };
    }
}

// ============================================================================
// EFFECTS
// ============================================================================

public class MaintenanceEffects
{
    private readonly IMediator _mediator;
    private readonly ILogger<MaintenanceEffects> _logger;

    public MaintenanceEffects(IMediator mediator, ILogger<MaintenanceEffects> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleSubmitMaintenanceRequest(
        SubmitMaintenanceRequestAction action,
        IDispatcher dispatcher)
    {
        try
        {
            // TODO: Implement optimistic update with Fluxor
            var command = new SubmitMaintenanceRequestCommand
            {
                PropertyId = action.PropertyId,
                UnitId = action.UnitId,
                Description = action.Description,
                UrgencyLevel = action.UrgencyLevel
            };

            var result = await _mediator.Send(command);

            if (result.Success && result.WorkOrderId.HasValue)
            {
                dispatcher.Dispatch(new SubmitMaintenanceRequestSuccessAction(result.WorkOrderId.Value));
            }
            else
            {
                var errorMessage = result.Errors.Any()
                    ? string.Join(", ", result.Errors)
                    : result.Message;

                dispatcher.Dispatch(new SubmitMaintenanceRequestFailureAction(errorMessage));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleSubmitMaintenanceRequest effect");
            dispatcher.Dispatch(new SubmitMaintenanceRequestFailureAction(
                "An unexpected error occurred"));
        }
    }
}
