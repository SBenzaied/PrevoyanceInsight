using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PrevoyanceInsight.Application.Common;
using PrevoyanceInsight.Application.Plans.Queries;
using PrevoyanceInsight.Infrastructure.Documents;
using PrevoyanceInsight.Infrastructure.Messaging;
using PrevoyanceInsight.Infrastructure.Persistence;

// Stdio (défaut) : Claude Desktop/Code lance ce process et lui parle par pipes, pas de
// port réseau — c'est ce que ce projet a toujours fait. HTTP : nécessaire pour un
// déploiement (Render n'expose que des ports réseau), activé par McpServer__Transport=Http
// sans toucher aux configurations MCP client locales existantes.
bool transportHttp = string.Equals(
    Environment.GetEnvironmentVariable("McpServer__Transport"),
    "Http",
    StringComparison.OrdinalIgnoreCase);

if (transportHttp)
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    ConfigurerServicesPartagees(builder);
    builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();

    WebApplication app = builder.Build();
    app.MapMcp();
    await app.RunAsync();
}
else
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
    ConfigurerServicesPartagees(builder);
    builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly();

    await builder.Build().RunAsync();
}

// Même pipeline CQRS que l'API REST : un agent (Claude Code, Claude Desktop,
// ou l'outil interne de l'actuariat) parle au même cœur applicatif, juste
// par un transport différent (stdio ou HTTP ici, toujours HTTP pour l'API REST).
static void ConfigurerServicesPartagees(IHostApplicationBuilder builder)
{
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
}

// Pour connecter ce serveur à Claude Desktop ou Claude Code en local, voir
// docs/ARCHITECTURE.md § "Brancher le serveur MCP".
