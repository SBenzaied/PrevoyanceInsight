using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using PrevoyanceInsight.Application.Common;

namespace PrevoyanceInsight.Infrastructure.Messaging
{
    /// <summary>
    /// Publie les événements métier sur RabbitMQ via MassTransit. Choix de MassTransit
    /// plutôt que le client RabbitMQ brut : il fournit le routage par type de message,
    /// les retries, et l'outbox pattern nécessaires pour ne pas perdre d'événement
    /// entre la command handler et le broker (important ici : un événement perdu
    /// = une anomalie actuarielle non notifiée).
    /// </summary>
    public class RabbitMqEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
    {
        public Task PublierAsync<TEvent>(TEvent evenement, CancellationToken ct = default) where TEvent : class =>
            publishEndpoint.Publish(evenement, ct);
    }

    /// <summary>
    /// Extension d'amorçage : enregistre MassTransit + RabbitMQ. Chaque module
    /// (Plans, Rapports) peut consommer les événements des autres sans référence
    /// directe — c'est ce qui permet de découper le monolithe modulaire en
    /// microservices indépendants plus tard sans changer les contrats.
    /// </summary>
    public static class MessagingServiceCollectionExtensions
    {
        public static IServiceCollection AjouterMessagerie(this IServiceCollection services, string rabbitMqHost)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqHost, "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
            return services;
        }
    }
}
