using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using PrevoyanceInsight.Application.Rapports.Commands;

namespace PrevoyanceInsight.McpServer.Tools
{
    [McpServerToolType]
    public static class RapportsTools
    {
        [McpServerTool, Description("Génère un rapport actuariel pour un plan de prévoyance, dans le format demandé (Pdf, Csv ou Json). Retourne les statistiques utilisées et un événement est publié sur le bus pour traçabilité.")]
        public static async Task<string> GenererRapport(
            ISender mediator,
            [Description("Identifiant du plan de prévoyance")] Guid planId,
            [Description("Format du rapport : Pdf, Csv ou Json")] FormatRapport format)
        {
            RapportGenere resultat = await mediator.Send(new GenererRapportCommand(planId, format));
            return System.Text.Json.JsonSerializer.Serialize(resultat);
        }
    }
}
