using MediatR;
using PrevoyanceInsight.Application.Common;
using PrevoyanceInsight.Domain.ValueObjects;

namespace PrevoyanceInsight.Application.Plans.Queries
{
    using PrevoyanceInsight.Domain.Entities;

    /// <summary>
    /// Query CQRS : ne modifie rien, retourne une comparaison entre deux plans.
    /// C'est cette query que le serveur MCP expose telle quelle sous forme d'outil
    /// "comparer_plans" pour que l'agent puisse la déclencher via prompting.
    /// </summary>
    public sealed record ComparerPlansQuery(Guid PlanIdA, Guid PlanIdB) : IRequest<ComparaisonPlans>;

    public sealed class ComparerPlansQueryHandler(IPlanRepository repository)
        : IRequestHandler<ComparerPlansQuery, ComparaisonPlans>
    {
        public async Task<ComparaisonPlans> Handle(ComparerPlansQuery request, CancellationToken ct)
        {
            PlanPrevoyance planA = await repository.ObtenirParIdAsync(request.PlanIdA, ct)
                                ?? throw new KeyNotFoundException($"Plan {request.PlanIdA} introuvable.");
            PlanPrevoyance planB = await repository.ObtenirParIdAsync(request.PlanIdB, ct)
                                ?? throw new KeyNotFoundException($"Plan {request.PlanIdB} introuvable.");

            decimal ecartCouverture = planA.TauxCouverture - planB.TauxCouverture;
            decimal ecartCotisation = planA.TauxCotisationTotal - planB.TauxCotisationTotal;
            decimal ecartTechnique = planA.TauxTechnique - planB.TauxTechnique;

            string synthese = ConstruireSynthese(planA.Nom, planB.Nom, ecartCouverture, ecartCotisation);

            return new ComparaisonPlans(
                planA.Nom,
                planB.Nom,
                ecartCouverture,
                ecartCotisation,
                ecartTechnique,
                synthese);
        }

        private static string ConstruireSynthese(string nomA, string nomB, decimal ecartCouverture, decimal ecartCotisation)
        {
            string comparaisonCouverture = ecartCouverture switch
                                           {
                                               > 0 => $"{nomA} affiche un taux de couverture supérieur de {ecartCouverture:P1} à {nomB}.",
                                               < 0 => $"{nomB} affiche un taux de couverture supérieur de {Math.Abs(ecartCouverture):P1} à {nomA}.",
                                               _ => $"{nomA} et {nomB} ont un taux de couverture identique."
                                           };

            string comparaisonCotisation = ecartCotisation switch
                                           {
                                               > 0 => $"{nomA} cotise {ecartCotisation:P1} de plus au total.",
                                               < 0 => $"{nomB} cotise {Math.Abs(ecartCotisation):P1} de plus au total.",
                                               _ => "Les taux de cotisation totaux sont identiques."
                                           };

            return $"{comparaisonCouverture} {comparaisonCotisation}";
        }
    }
}
