using PrevoyanceInsight.Application.Common;
using Raven.Client.Documents;

namespace PrevoyanceInsight.Infrastructure.Documents
{
    using Raven.Client.Documents.Session;

    /// <summary>
    /// Stocke le texte réglementaire (et ses versions) d'un plan de prévoyance dans
    /// RavenDB, choisi ici plutôt qu'une table PostgreSQL car ce contenu est peu
    /// structuré, versionné, et interrogé par contenu plutôt que par jointure —
    /// un usage typique où un magasin NoSQL orienté document est pertinent
    /// à côté du relationnel plutôt qu'à sa place.
    /// </summary>
    public class RavenDbReglementDocumentStore(IDocumentStore store) : IReglementDocumentStore
    {
        public async Task<string> EnregistrerAsync(string planNom, string contenuTexte, CancellationToken ct = default)
        {
            using IAsyncDocumentSession? session = store.OpenAsyncSession();
            ReglementDocument document = new()
            {
                PlanNom = planNom,
                Contenu = contenuTexte,
                Version = 1,
                EnregistreLe = DateTimeOffset.UtcNow
            };

            await session.StoreAsync(document, ct);
            await session.SaveChangesAsync(ct);
            return document.Id!;
        }

        public async Task<string?> LireAsync(string documentId, CancellationToken ct = default)
        {
            using IAsyncDocumentSession? session = store.OpenAsyncSession();
            ReglementDocument? document = await session.LoadAsync<ReglementDocument>(documentId, ct);
            return document?.Contenu;
        }
    }

    public class ReglementDocument
    {
        public string? Id { get; set; }
        public string PlanNom { get; set; } = null!;
        public string Contenu { get; set; } = null!;
        public int Version { get; set; }
        public DateTimeOffset EnregistreLe { get; set; }
    }
}
