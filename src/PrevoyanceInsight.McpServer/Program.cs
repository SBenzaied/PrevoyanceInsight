using MediatR;
using Microsoft.EntityFrameworkCore;
using PrevoyanceInsight.Application.Common;
using PrevoyanceInsight.Application.Plans.Queries;
using PrevoyanceInsight.Infrastructure.Documents;
using PrevoyanceInsight.Infrastructure.Messaging;
using PrevoyanceInsight.Infrastructure.Persistence;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Même pipeline CQRS que l'API REST : un agent (Claude Code, Claude Desktop,
// ou l'outil interne de l'actuariat) parle au même cœur applicatif, juste
// par un transport différent (stdio ici, HTTP pour l'API).
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ComparerPlansQuery).Assembly));

builder.Services.AddDbContext<PrevoyanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AjouterMessagerie(builder.Configuration["RabbitMq:ConnectionString"] ?? "localhost");
builder.Services.AjouterDocuments(
    builder.Configuration.GetSection("RavenDb:Urls").Get<string[]>() ?? ["http://localhost:8080"],
    builder.Configuration["RavenDb:Database"] ?? "prevoyance-reglements",
    builder.Configuration["RavenDb:CertificatePath"],
    builder.Configuration["RavenDb:CertificateBase64"],
    builder.Configuration["RavenDb:CertificatePassword"]);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

// Pour connecter ce serveur à Claude Desktop ou Claude Code, voir
// docs/ARCHITECTURE.md § "Brancher le serveur MCP".
