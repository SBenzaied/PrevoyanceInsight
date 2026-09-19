using MediatR;
using PrevoyanceInsight.Application.Common;

namespace PrevoyanceInsight.Application.Plans.Commands
{
    using PrevoyanceInsight.Domain.Entities;

    /// <summary>
    /// Command CQRS : effectue un contrôle de masse sur les bénéficiaires d'un plan
    /// et publie un événement par anomalie détectée, plutôt que de tout traiter en
    /// mémoire — pour rester scalable quand le volume d'assurés grandit.
    /// C'est cette command que le serveur MCP expose comme outil "detecter_anomalies".
    /// </summary>
    public sealed record DetecterAnomaliesCommand(Guid PlanId) : IRequest<RapportAnomalies>;

    public sealed record RapportAnomalies(Guid PlanId, int NombreAssuresControles, int NombreAnomalies, IReadOnlyList<string> Motifs);

    public sealed class DetecterAnomaliesCommandHandler(IPlanRepository repository, IEventPublisher eventPublisher)
        : IRequestHandler<DetecterAnomaliesCommand, RapportAnomalies>
    {
        public async Task<RapportAnomalies> Handle(DetecterAnomaliesCommand request, CancellationToken ct)
        {
            IReadOnlyList<Beneficiaire> beneficiaires = await repository.ObtenirBeneficiairesAsync(request.PlanId, ct);
            List<string> motifs = [];

            foreach (Beneficiaire beneficiaire in beneficiaires.Where(b => b.EstAnomalie()))
            {
                string motif = $"Avoir vieillesse nul pour un assuré actif (id {beneficiaire.Id}).";
                motifs.Add(motif);

                await eventPublisher.PublierAsync(
                    new AnomalieDetecteeEvent(request.PlanId, beneficiaire.Id, motif, DateTimeOffset.UtcNow),
                    ct);
            }

            return new RapportAnomalies(request.PlanId, beneficiaires.Count, motifs.Count, motifs);
        }
    }
}
