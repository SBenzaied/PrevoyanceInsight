using MassTransit;
using Microsoft.Extensions.Logging;
using PrevoyanceInsight.Application.Common;

namespace PrevoyanceInsight.Infrastructure.Messaging
{
    /// <summary>
    /// Consommateur de démonstration : matérialise le bout de la chaîne événementielle
    /// (publish RabbitMQ → queue → consumer) qui n'était jusque-là que déclarative.
    /// Un vrai module de notification/audit ferait ici l'envoi d'alerte ou l'écriture
    /// dans un journal d'audit ; on se contente de logguer pour la démonstration.
    /// </summary>
    public class AnomalieDetecteeConsumer(ILogger<AnomalieDetecteeConsumer> logger) : IConsumer<AnomalieDetecteeEvent>
    {
        public Task Consume(ConsumeContext<AnomalieDetecteeEvent> context)
        {
            AnomalieDetecteeEvent evenement = context.Message;
            logger.LogWarning(
                "Anomalie détectée — plan {PlanId}, bénéficiaire {BeneficiaireId} : {Motif} (détectée le {DetecteeLe})",
                evenement.PlanId, evenement.BeneficiaireId, evenement.Motif, evenement.DetecteeLe);
            return Task.CompletedTask;
        }
    }
}
