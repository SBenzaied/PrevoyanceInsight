namespace PrevoyanceInsight.Domain.Entities
{
    /// <summary>
    /// Représente un assuré actif ou pensionné rattaché à un plan de prévoyance.
    /// Anonymisé par construction : aucune donnée nominative n'est modélisée ici,
    /// seulement ce qui est nécessaire aux calculs actuariels et aux contrôles de masse.
    /// </summary>
    public class Beneficiaire
    {
        public Guid Id { get; private set; }
        public Guid PlanId { get; private set; }
        public int AnneeNaissance { get; private set; }
        public StatutAssure Statut { get; private set; }
        public decimal SalaireAssure { get; private set; }
        public decimal AvoirVieillesse { get; private set; }

        public Beneficiaire(Guid planId, int anneeNaissance, StatutAssure statut, decimal salaireAssure, decimal avoirVieillesse)
        {
            this.Id = Guid.NewGuid();
            this.PlanId = planId;
            this.AnneeNaissance = anneeNaissance;
            this.Statut = statut;
            this.SalaireAssure = salaireAssure;
            this.AvoirVieillesse = avoirVieillesse;
        }

        public int Age(int anneeReference) => anneeReference - this.AnneeNaissance;

        /// <summary>
        /// Règle de contrôle de masse simple : un avoir vieillesse à zéro pour un assuré
        /// actif est une anomalie de saisie ou d'import CDC à investiguer.
        /// </summary>
        public bool EstAnomalie() => this.Statut == StatutAssure.Actif && this.AvoirVieillesse <= 0;
    }

    public enum StatutAssure
    {
        Actif,
        Pensionne,
        Invalide
    }
}
