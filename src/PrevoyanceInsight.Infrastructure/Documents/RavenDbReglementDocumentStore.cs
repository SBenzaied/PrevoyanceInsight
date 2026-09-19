using Microsoft.Extensions.DependencyInjection;
using PrevoyanceInsight.Application.Common;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System.Security.Cryptography.X509Certificates;

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

    /// <summary>
    /// Extension d'amorçage : enregistre le IDocumentStore RavenDB. RavenDB Cloud
    /// s'authentifie par certificat client X.509 (pas d'utilisateur/mot de passe dans
    /// l'URL comme Postgres/RabbitMQ) — le chemin du .pfx et son mot de passe viennent
    /// de la config (user-secrets en local, variables d'environnement en prod), jamais
    /// du repo.
    /// </summary>
    public static class DocumentsServiceCollectionExtensions
    {
        public static IServiceCollection AjouterDocuments(
            this IServiceCollection services,
            string[] urls,
            string database,
            string? certificatePath,
            string? certificateBase64,
            string? certificatePassword)
        {
            services.AddSingleton<IDocumentStore>(_ =>
            {
                DocumentStore store = new()
                {
                    Urls = urls,
                    Database = database
                };

                // En hébergement (Render, etc.) le fichier .pfx ne peut pas être déposé
                // sur le disque du conteneur — on le passe en base64 via une variable
                // d'environnement plutôt qu'un chemin de fichier, qui reste réservé au
                // poste de dev local.
                if (!string.IsNullOrEmpty(certificateBase64))
                {
                    store.Certificate = new X509Certificate2(Convert.FromBase64String(certificateBase64), certificatePassword);
                }
                else if (!string.IsNullOrEmpty(certificatePath))
                {
                    store.Certificate = new X509Certificate2(certificatePath, certificatePassword);
                }

                store.Initialize();

                // RavenDB, contrairement à EF Core sur Postgres, ne crée pas la base au
                // premier accès — on le fait ici pour que le déploiement reste sans étape
                // manuelle dans le Studio.
                if (!store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, int.MaxValue)).Contains(database))
                {
                    try
                    {
                        store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(database)));
                    }
                    catch (ConcurrencyException)
                    {
                        // créée entretemps par une autre instance — rien à faire.
                    }
                }

                return store;
            });

            services.AddScoped<IReglementDocumentStore, RavenDbReglementDocumentStore>();
            return services;
        }
    }
}
