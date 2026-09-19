using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using PrevoyanceInsight.Application.Common;
using System.Security.Authentication;

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
        public static IServiceCollection AjouterMessagerie(this IServiceCollection services, string rabbitMqConnectionString)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    // CloudAMQP (et tout broker managé) expose une URI amqp(s)://user:pass@host/vhost
                    // plutôt qu'un simple nom d'hôte — on la décompose nous-mêmes car les overloads
                    // Host(string) de MassTransit ne parsent pas les credentials/vhost intégrés à l'URI.
                    if (Uri.TryCreate(rabbitMqConnectionString, UriKind.Absolute, out Uri? uri) &&
                        (uri.Scheme == "amqp" || uri.Scheme == "amqps"))
                    {
                        string virtualHost = string.IsNullOrEmpty(uri.AbsolutePath) || uri.AbsolutePath == "/"
                            ? "/"
                            : Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
                        ushort port = (ushort)(uri.IsDefaultPort ? (uri.Scheme == "amqps" ? 5671 : 5672) : uri.Port);
                        string[] userInfo = uri.UserInfo.Split(':', 2);

                        cfg.Host(uri.Host, port, virtualHost, h =>
                        {
                            h.Username(Uri.UnescapeDataString(userInfo[0]));
                            h.Password(userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty);

                            if (uri.Scheme == "amqps")
                            {
                                h.UseSsl(s => s.Protocol = SslProtocols.Tls12);
                            }
                        });
                    }
                    else
                    {
                        cfg.Host(rabbitMqConnectionString, "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });
                    }

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
            return services;
        }
    }
}
