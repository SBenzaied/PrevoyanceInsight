using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using PrevoyanceInsight.Application.Common;
using PrevoyanceInsight.Application.Plans.Commands;
using PrevoyanceInsight.Application.Plans.Queries;

namespace PrevoyanceInsight.McpServer.Tools
{
    using PrevoyanceInsight.Domain.Entities;
    using PrevoyanceInsight.Domain.ValueObjects;

    /// <summary>
    /// Outils MCP exposés à un agent (Claude, ou tout client MCP). Chaque outil
    /// délègue à MediatR : la logique métier vit dans PrevoyanceInsight.Application,
    /// le serveur MCP n'est qu'une nouvelle porte d'entrée — exactement comme l'API
    /// REST. C'est ce qui répond à l'objectif 2 de l'offre : permettre à l'actuariat
    /// de "produire et compléter les rapports" et de "comparer les plans" via prompting.
    /// </summary>
    [McpServerToolType]
    public static class PlansTools
    {
        [McpServerTool, Description("Compare deux plans de prévoyance et retourne les écarts de taux de couverture, de cotisation et technique, avec une synthèse en langage naturel.")]
        public static async Task<string> ComparerPlans(
            ISender mediator,
            [Description("Identifiant du premier plan de prévoyance")] Guid planIdA,
            [Description("Identifiant du second plan de prévoyance")] Guid planIdB)
        {
            ComparaisonPlans resultat = await mediator.Send(new ComparerPlansQuery(planIdA, planIdB));
            return System.Text.Json.JsonSerializer.Serialize(resultat);
        }

        [McpServerTool, Description("Retourne les statistiques d'un plan de prévoyance : effectifs par statut, âge moyen, avoir vieillesse moyen, taux de couverture.")]
        public static async Task<string> ObtenirStatistiques(
            ISender mediator,
            [Description("Identifiant du plan de prévoyance")] Guid planId)
        {
            StatistiquesPlan resultat = await mediator.Send(new ObtenirStatistiquesPlanQuery(planId));
            return System.Text.Json.JsonSerializer.Serialize(resultat);
        }

        [McpServerTool, Description("Effectue un contrôle de masse sur les bénéficiaires d'un plan et retourne les anomalies détectées (ex : avoir vieillesse nul pour un assuré actif).")]
        public static async Task<string> DetecterAnomalies(
            ISender mediator,
            [Description("Identifiant du plan de prévoyance à contrôler")] Guid planId)
        {
            RapportAnomalies resultat = await mediator.Send(new DetecterAnomaliesCommand(planId));
            return System.Text.Json.JsonSerializer.Serialize(resultat);
        }

        [McpServerTool, Description("Liste tous les plans de prévoyance gérés, avec leur type de primauté et leur taux de couverture.")]
        public static async Task<string> ListerPlans(IPlanRepository repository)
        {
            IReadOnlyList<PlanPrevoyance> plans = await repository.ListerTousAsync();
            var resume = plans.Select(p => new { p.Id, p.Nom, p.Primaute, p.TauxCouverture, p.NombreAssures });
            return System.Text.Json.JsonSerializer.Serialize(resume);
        }
    }
}
