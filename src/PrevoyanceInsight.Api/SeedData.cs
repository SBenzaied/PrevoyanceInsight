using PrevoyanceInsight.Domain.Entities;
using PrevoyanceInsight.Infrastructure.Persistence;

namespace PrevoyanceInsight.Api
{
    /// <summary>
    /// Jeu de données de démonstration : deux plans de prévoyance contrastés (cadres /
    /// employés) avec quelques bénéficiaires, dont une anomalie volontaire, pour que
    /// les outils MCP et l'API aient quelque chose à comparer dès le premier lancement.
    /// </summary>
    public static class SeedData
    {
        public static async Task EnsureSeededAsync(PrevoyanceDbContext db)
        {
            if (db.Plans.Any())
            {
                return; // déjà amorcé
            }

            PlanPrevoyance planCadres = new PlanPrevoyance("Plan Cadres", TypePrimaute.Cotisations,
                                                           tauxCouverture: 0.95m, tauxCotisationEmployeur: 0.10m, tauxCotisationEmploye: 0.08m,
                                                           tauxTechnique: 0.022m, nombreAssures: 420, dateEntreeVigueur: new DateOnly(2018, 1, 1));

            PlanPrevoyance planEmployes = new PlanPrevoyance("Plan Employés", TypePrimaute.Cotisations,
                                                             tauxCouverture: 0.82m, tauxCotisationEmployeur: 0.08m, tauxCotisationEmploye: 0.06m,
                                                             tauxTechnique: 0.020m, nombreAssures: 1150, dateEntreeVigueur: new DateOnly(2018, 1, 1));

            db.Plans.AddRange(planCadres, planEmployes);

            db.Beneficiaires.AddRange(
                new Beneficiaire(planCadres.Id, 1978, StatutAssure.Actif, 145000m, avoirVieillesse: 310000m),
                new Beneficiaire(planCadres.Id, 1985, StatutAssure.Actif, 120000m, avoirVieillesse: 210000m),
                new Beneficiaire(planCadres.Id, 1960, StatutAssure.Pensionne, 0m, avoirVieillesse: 0m),
                new Beneficiaire(planEmployes.Id, 1990, StatutAssure.Actif, 68000m, avoirVieillesse: 45000m),
                new Beneficiaire(planEmployes.Id, 1995, StatutAssure.Actif, 62000m, avoirVieillesse: 0m), // anomalie volontaire
                new Beneficiaire(planEmployes.Id, 1958, StatutAssure.Invalide, 0m, avoirVieillesse: 15000m)
            );

            await db.SaveChangesAsync();
        }
    }
}
