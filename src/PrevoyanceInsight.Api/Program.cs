using MediatR;
using Microsoft.EntityFrameworkCore;
using PrevoyanceInsight.Api;
using PrevoyanceInsight.Application.Common;
using PrevoyanceInsight.Application.Plans.Commands;
using PrevoyanceInsight.Application.Plans.Queries;
using PrevoyanceInsight.Application.Rapports.Commands;
using PrevoyanceInsight.Infrastructure.Documents;
using PrevoyanceInsight.Infrastructure.Messaging;
using PrevoyanceInsight.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// CQRS : un seul point d'entrée MediatR pour toutes les commands/queries,
// que l'API REST et le serveur MCP partagent — pas de logique dupliquée entre les deux entrées.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ComparerPlansQuery).Assembly));

builder.Services.AddDbContext<PrevoyanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AjouterMessagerie(
    builder.Configuration["RabbitMq:ConnectionString"] ?? "localhost",
    x => x.AddConsumer<AnomalieDetecteeConsumer>());
builder.Services.AjouterDocuments(
    builder.Configuration.GetSection("RavenDb:Urls").Get<string[]>() ?? ["http://localhost:8080"],
    builder.Configuration["RavenDb:Database"] ?? "prevoyance-reglements",
    builder.Configuration["RavenDb:CertificatePath"],
    builder.Configuration["RavenDb:CertificateBase64"],
    builder.Configuration["RavenDb:CertificatePassword"]);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    PrevoyanceDbContext db = scope.ServiceProvider.GetRequiredService<PrevoyanceDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureSeededAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// REST API "classique" — c'est la même Application layer que le serveur MCP,
// simplement exposée en HTTP plutôt qu'en tools d'agent.

app.MapGet("/api/plans", async (ISender mediator, IPlanRepository repo, CancellationToken ct) =>
    Results.Ok(await repo.ListerTousAsync(ct)));

app.MapGet("/api/plans/{id:guid}/statistiques", async (Guid id, ISender mediator, CancellationToken ct) =>
    Results.Ok(await mediator.Send(new ObtenirStatistiquesPlanQuery(id), ct)));

app.MapGet("/api/plans/comparer", async (Guid planA, Guid planB, ISender mediator, CancellationToken ct) =>
    Results.Ok(await mediator.Send(new ComparerPlansQuery(planA, planB), ct)));

app.MapPost("/api/plans/{id:guid}/anomalies", async (Guid id, ISender mediator, CancellationToken ct) =>
    Results.Ok(await mediator.Send(new DetecterAnomaliesCommand(id), ct)));

app.MapPost("/api/plans/{id:guid}/rapports", async (Guid id, FormatRapport format, ISender mediator, CancellationToken ct) =>
    Results.Ok(await mediator.Send(new GenererRapportCommand(id, format), ct)));

app.Run();