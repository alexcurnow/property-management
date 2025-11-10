using FluentValidation;
using Fluxor;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PropertyManagement.Web.Common.Behaviors;
using PropertyManagement.Web.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MudBlazor
builder.Services.AddMudServices();

// PostgreSQL with Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=PropertyManagement;Username=postgres;Password=postgres";

builder.Services.AddDbContext<PropertyManagementDbContext>(options =>
    options.UseNpgsql(connectionString));

// MediatR for CQRS
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);

    // Add logging behavior to the pipeline
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Fluxor for state management
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    // TODO: Install Fluxor.Blazor.Web.ReduxDevTools package to enable Redux DevTools
    // options.UseReduxDevTools(); // Enable Redux DevTools for debugging
});

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<PropertyManagement.Web.App>()
    .AddInteractiveServerRenderMode();

// Ensure database is created (for development only)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PropertyManagementDbContext>();

    if (app.Environment.IsDevelopment())
    {
        // In development, apply migrations automatically
        // TODO: In production, use proper migration strategy
        dbContext.Database.EnsureCreated();
    }
}

app.Run();
