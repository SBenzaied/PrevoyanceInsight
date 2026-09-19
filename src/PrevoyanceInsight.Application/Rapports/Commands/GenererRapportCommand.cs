using MediatR;
using PrevoyanceInsight.Application.Common;
using PrevoyanceInsight.Application.Plans.Queries;

namespace PrevoyanceInsight.Application.Rapports.Commands
{
    /// <summary>
    /// Command CQRS : orchestre la génération d'un rapport pour l'actuariat, en
    /// combinant les statistiques du plan et un texte de synthèse en langage
    /// naturel (généré par l'IA en aval, voir McpServer/Tools/GenererRapportTool).
    /// C'est ce point d'entrée que l'objectif 2 de l'offre ("compléter les
    /// rapports via prompting") vient déclencher.
    /// </summary>
    public sealed record GenererRapportCommand(Guid PlanId, FormatRapport Format) : IRequest<RapportGenere>;

    public enum FormatRapport { Pdf, Csv, Json }

    public sealed record RapportGenere(Guid PlanId, FormatRapport Format, StatistiquesPlan Statistiques, DateTimeOffset GenereLe);

    public sealed class GenererRapportCommandHandler(ISender mediator, IEventPublisher eventPublisher)
        : IRequestHandler<GenererRapportCommand, RapportGenere>
    {
        public async Task<RapportGenere> Handle(GenererRapportCommand request, CancellationToken ct)
        {
            // Réutilise la query existante plutôt que de dupliquer l'accès aux données :
            // le pattern CQRS n'empêche pas une command de composer une query en lecture.
            StatistiquesPlan statistiques = await mediator.Send(new ObtenirStatistiquesPlanQuery(request.PlanId), ct);

            await eventPublisher.PublierAsync(
                new RapportGenereEvent(request.PlanId, request.Format.ToString(), DateTimeOffset.UtcNow),
                ct);

            return new RapportGenere(request.PlanId, request.Format, statistiques, DateTimeOffset.UtcNow);
        }
    }
}
