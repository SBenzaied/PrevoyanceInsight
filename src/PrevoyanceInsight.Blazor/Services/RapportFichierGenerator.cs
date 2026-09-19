using System.Globalization;
using System.Text;
using System.Text.Json;
using PrevoyanceInsight.Application.Plans.Queries;
using PrevoyanceInsight.Application.Rapports.Commands;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PrevoyanceInsight.Blazor.Services
{
    /// <summary>
    /// Produit le fichier téléchargeable (bytes) correspondant à un RapportGenere,
    /// dans le format demandé. GenererRapportCommand ne renvoie que les métadonnées
    /// (réutilisées telles quelles par le serveur MCP) ; le rendu binaire du fichier
    /// est un besoin propre à l'UI Blazor, donc il vit ici plutôt que dans Application.
    /// </summary>
    public static class RapportFichierGenerator
    {
        public sealed record FichierRapport(byte[] Contenu, string ContentType, string NomFichier);

        public static FichierRapport Generer(RapportGenere rapport)
        {
            string nomSlug = Slugify(rapport.Statistiques.NomPlan);
            string date = rapport.GenereLe.ToLocalTime().ToString("yyyy-MM-dd_HHmm", CultureInfo.InvariantCulture);

            return rapport.Format switch
            {
                FormatRapport.Json => new FichierRapport(
                    JsonSerializer.SerializeToUtf8Bytes(rapport, new JsonSerializerOptions { WriteIndented = true }),
                    "application/json",
                    $"rapport-{nomSlug}-{date}.json"),

                FormatRapport.Csv => new FichierRapport(
                    Encoding.UTF8.GetBytes(GenererCsv(rapport)),
                    "text/csv",
                    $"rapport-{nomSlug}-{date}.csv"),

                FormatRapport.Pdf => new FichierRapport(
                    GenererPdf(rapport),
                    "application/pdf",
                    $"rapport-{nomSlug}-{date}.pdf"),

                _ => throw new NotSupportedException($"Format de rapport non supporté : {rapport.Format}")
            };
        }

        private static string GenererCsv(RapportGenere rapport)
        {
            StatistiquesPlan s = rapport.Statistiques;
            StringBuilder csv = new();
            csv.AppendLine("Indicateur,Valeur");
            csv.AppendLine($"Plan,{s.NomPlan}");
            csv.AppendLine($"Généré le,{rapport.GenereLe.ToLocalTime():yyyy-MM-dd HH:mm}");
            csv.AppendLine($"Actifs,{s.NombreActifs}");
            csv.AppendLine($"Pensionnés,{s.NombrePensionnes}");
            csv.AppendLine($"Invalides,{s.NombreInvalides}");
            csv.AppendLine($"Âge moyen (actifs),{s.AgeMoyenActifs.ToString("N1", CultureInfo.InvariantCulture)}");
            csv.AppendLine($"Avoir vieillesse moyen,{s.AvoirVieillesseMoyen.ToString("N0", CultureInfo.InvariantCulture)}");
            csv.AppendLine($"Taux de couverture,{s.TauxCouverture.ToString("P1", CultureInfo.InvariantCulture)}");
            return csv.ToString();
        }

        private static byte[] GenererPdf(RapportGenere rapport)
        {
            StatistiquesPlan s = rapport.Statistiques;
            (string Label, string Value)[] lignes =
            [
                ("Actifs", s.NombreActifs.ToString(CultureInfo.InvariantCulture)),
                ("Pensionnés", s.NombrePensionnes.ToString(CultureInfo.InvariantCulture)),
                ("Invalides", s.NombreInvalides.ToString(CultureInfo.InvariantCulture)),
                ("Âge moyen (actifs)", s.AgeMoyenActifs.ToString("N1", CultureInfo.InvariantCulture)),
                ("Avoir vieillesse moyen", s.AvoirVieillesseMoyen.ToString("N0", CultureInfo.InvariantCulture)),
                ("Taux de couverture", s.TauxCouverture.ToString("P1", CultureInfo.InvariantCulture)),
            ];

            Document document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("Rapport de synthèse — PrevoyanceInsight").FontSize(18).Bold();
                        column.Item().Text(s.NomPlan).FontSize(13).FontColor(Colors.Grey.Darken2);
                        column.Item().PaddingTop(2).Text($"Généré le {rapport.GenereLe.ToLocalTime():dd.MM.yyyy à HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        foreach ((string label, string value) in lignes)
                        {
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(label);
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(value).Bold();
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("PrevoyanceInsight").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static string Slugify(string valeur)
        {
            string sansAccents = string.Concat(valeur.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

            char[] caracteres = sansAccents
                .Select(c => char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-')
                .ToArray();

            string slug = new string(caracteres).Trim('-');
            while (slug.Contains("--", StringComparison.Ordinal))
            {
                slug = slug.Replace("--", "-");
            }

            return string.IsNullOrEmpty(slug) ? "plan" : slug;
        }
    }
}
