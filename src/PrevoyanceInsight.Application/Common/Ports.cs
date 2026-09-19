using PrevoyanceInsight.Domain.Entities;

namespace PrevoyanceInsight.Application.Common
{
    /// <summary>
    /// Port vers le stockage relationnel (PostgreSQL). Les query handlers lisent
    /// via ce repository ; les données structurées (taux, cotisations, effectifs)
    /// vivent ici, alimentées par le pipeline CDC décrit dans docs/ARCHITECTURE.md.
    /// </summary>
    public interface IPlanRepository
    {
        Task<PlanPrevoyance?> ObtenirParIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<PlanPrevoyance>> ListerTousAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Beneficiaire>> ObtenirBeneficiairesAsync(Guid planId, CancellationToken ct = default);
        Task AjouterAsync(PlanPrevoyance plan, CancellationToken ct = default);
    }

    /// <summary>
    /// Port vers le stockage documentaire (RavenDB) : règlements de plans, annexes,
    /// versions successives d'un document — données peu structurées, à la maille
    /// document plutôt que ligne/colonne.
    /// </summary>
    public interface IReglementDocumentStore
    {
        Task<string> EnregistrerAsync(string planNom, string contenuTexte, CancellationToken ct = default);
        Task<string?> LireAsync(string documentId, CancellationToken ct = default);
    }

    /// <summary>
    /// Port vers le message broker (RabbitMQ via MassTransit). Publie les événements
    /// métier consommés par les autres modules (ex : notifier le module Reporting
    /// qu'une anomalie a été détectée, sans coupler directement les deux modules).
    /// </summary>
    public interface IEventPublisher
    {
        Task PublierAsync<TEvent>(TEvent evenement, CancellationToken ct = default) where TEvent : class;
    }

    public sealed record AnomalieDetecteeEvent(Guid PlanId, Guid BeneficiaireId, string Motif, DateTimeOffset DetecteeLe);

    public sealed record RapportGenereEvent(Guid PlanId, string FormatRapport, DateTimeOffset GenereLe);
}
