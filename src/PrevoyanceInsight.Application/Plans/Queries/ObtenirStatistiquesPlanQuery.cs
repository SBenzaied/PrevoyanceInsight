using MediatR;
using PrevoyanceInsight.Application.Common;

namespace PrevoyanceInsight.Application.Plans.Queries
{
    using PrevoyanceInsight.Domain.Entities;

    public sealed record ObtenirStatistiquesPlanQuery(Guid PlanId) : IRequest<StatistiquesPlan>;

    public sealed record StatistiquesPlan(
        string NomPlan,
        int NombreActifs,
        int NombrePensionnes,
        int NombreInvalides,
        decimal AgeMoyenActifs,
        decimal AvoirVieillesseMoyen,
        decimal TauxCouverture);

    public sealed class ObtenirStatistiquesPlanQueryHandler(IPlanRepository repository)
        : IRequestHandler<ObtenirStatistiquesPlanQuery, StatistiquesPlan>
    {
        public async Task<StatistiquesPlan> Handle(ObtenirStatistiquesPlanQuery request, CancellationToken ct)
        {
            PlanPrevoyance plan = await repository.ObtenirParIdAsync(request.PlanId, ct)
                               ?? throw new KeyNotFoundException($"Plan {request.PlanId} introuvable.");
            IReadOnlyList<Beneficiaire> beneficiaires = await repository.ObtenirBeneficiairesAsync(request.PlanId, ct);

            int anneeReference = DateTime.UtcNow.Year;
            List<Beneficiaire> actifs = beneficiaires.Where(b => b.Statut == Domain.Entities.StatutAssure.Actif).ToList();

            return new StatistiquesPlan(
                NomPlan: plan.Nom,
                NombreActifs: actifs.Count,
                NombrePensionnes: beneficiaires.Count(b => b.Statut == Domain.Entities.StatutAssure.Pensionne),
                NombreInvalides: beneficiaires.Count(b => b.Statut == Domain.Entities.StatutAssure.Invalide),
                AgeMoyenActifs: actifs.Count == 0 ? 0 : (decimal)actifs.Average(b => b.Age(anneeReference)),
                AvoirVieillesseMoyen: beneficiaires.Count == 0 ? 0 : beneficiaires.Average(b => b.AvoirVieillesse),
                TauxCouverture: plan.TauxCouverture);
        }
    }
}
