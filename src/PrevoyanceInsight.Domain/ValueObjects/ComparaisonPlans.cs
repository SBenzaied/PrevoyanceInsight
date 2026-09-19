namespace PrevoyanceInsight.Domain.ValueObjects
{
    /// <summary>
    /// Value object immuable représentant le résultat d'une comparaison entre deux
    /// plans de prévoyance. Conçu pour être sérialisé tel quel en sortie du serveur MCP
    /// ou de l'API REST, sans dépendre du modèle EF Core.
    /// </summary>
    public sealed record ComparaisonPlans(
        string NomPlanA,
        string NomPlanB,
        decimal EcartTauxCouverture,
        decimal EcartTauxCotisationTotal,
        decimal EcartTauxTechnique,
        string Synthese);
}
