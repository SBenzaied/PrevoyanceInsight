namespace PrevoyanceInsight.Domain.Entities
{
    /// <summary>
    /// Représente un plan de prévoyance géré par l'institution (primauté des cotisations
    /// ou des prestations). C'est l'agrégat racine autour duquel s'organisent les
    /// comparaisons et les rapports statistiques.
    /// </summary>
    public class PlanPrevoyance
    {
        public Guid Id { get; private set; }
        public string Nom { get; private set; }
        public TypePrimaute Primaute { get; private set; }
        public decimal TauxCouverture { get; private set; }
        public decimal TauxCotisationEmployeur { get; private set; }
        public decimal TauxCotisationEmploye { get; private set; }
        public decimal TauxTechnique { get; private set; }
        public int NombreAssures { get; private set; }
        public DateOnly DateEntreeVigueur { get; private set; }

        /// <summary>
        /// Identifiant du document RavenDB contenant le règlement complet du plan
        /// (texte réglementaire, versions, annexes). Les données structurées vivent
        /// en PostgreSQL, le contenu documentaire non structuré vit en RavenDB.
        /// </summary>
        public string? ReglementDocumentId { get; private set; }

        public PlanPrevoyance(
            string nom,
            TypePrimaute primaute,
            decimal tauxCouverture,
            decimal tauxCotisationEmployeur,
            decimal tauxCotisationEmploye,
            decimal tauxTechnique,
            int nombreAssures,
            DateOnly dateEntreeVigueur)
        {
            if (string.IsNullOrWhiteSpace(nom))
            {
                throw new ArgumentException("Le nom du plan est obligatoire.", nameof(nom));
            }

            if (tauxCouverture < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tauxCouverture));
            }

            this.Id = Guid.NewGuid();
            this.Nom = nom;
            this.Primaute = primaute;
            this.TauxCouverture = tauxCouverture;
            this.TauxCotisationEmployeur = tauxCotisationEmployeur;
            this.TauxCotisationEmploye = tauxCotisationEmploye;
            this.TauxTechnique = tauxTechnique;
            this.NombreAssures = nombreAssures;
            this.DateEntreeVigueur = dateEntreeVigueur;
        }

        public void LierReglement(string documentId) => this.ReglementDocumentId = documentId;

        public decimal TauxCotisationTotal => this.TauxCotisationEmployeur + this.TauxCotisationEmploye;

        /// <summary>
        /// Un plan est jugé "à risque" si son taux de couverture est sous le seuil légal
        /// de 80% visé par la Confédération, ou si son taux technique s'écarte fortement
        /// de la référence de la Chambre suisse des experts en caisses de pensions.
        /// </summary>
        public bool EstSousSurveillance(decimal seuilCouverture = 0.80m) => this.TauxCouverture < seuilCouverture;
    }

    public enum TypePrimaute
    {
        Cotisations,
        Prestations
    }
}
