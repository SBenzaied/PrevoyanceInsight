using MediatR;
using Microsoft.EntityFrameworkCore;
using PrevoyanceInsight.Application.Common;
using PrevoyanceInsight.Application.Plans.Queries;
using PrevoyanceInsight.Blazor.Components;
using PrevoyanceInsight.Infrastructure.Messaging;
using PrevoyanceInsight.Infrastructure.Persistence;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Même Application layer que l'API et le serveur MCP — le dashboard ne fait
// qu'ajouter une vue Blazor par-dessus les mêmes queries/repositories.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ComparerPlansQuery).Assembly));

builder.Services.AddDbContext<PrevoyanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IPlanRepository, PlanRepository>();

// Le scan d'assembly de MediatR enregistre aussi les handlers de Rapports/Anomalies
// (non utilisés par ce dashboard) — ils dépendent d'IEventPublisher, donc il doit
// être enregistré pour que le conteneur DI se construise.
builder.Services.AjouterMessagerie(builder.Configuration["RabbitMq:ConnectionString"] ?? "localhost");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
