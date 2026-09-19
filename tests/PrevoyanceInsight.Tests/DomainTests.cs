namespace PrevoyanceInsight.Tests
{
    using NUnit.Framework;
    using PrevoyanceInsight.Domain.Entities;

    [TestFixture]
    public class PlanPrevoyanceTests
    {
        [Test]
        public void Constructeur_QuandNomVide_LeveArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new PlanPrevoyance(string.Empty, TypePrimaute.Cotisations, 0.9m, 0.08m, 0.08m, 0.02m, 100, new DateOnly(2020, 1, 1)));
        }

        [TestCase(0.75, ExpectedResult = true)]
        [TestCase(0.80, ExpectedResult = false)]
        [TestCase(0.95, ExpectedResult = false)]
        public bool EstSousSurveillance_SelonLeTauxDeCouverture(decimal tauxCouverture)
        {
            PlanPrevoyance plan = new("Plan Test", TypePrimaute.Cotisations, tauxCouverture, 0.08m, 0.08m, 0.02m, 100, new DateOnly(2020, 1, 1));
            return plan.EstSousSurveillance();
        }

        [Test]
        public void TauxCotisationTotal_AdditionneEmployeurEtEmploye()
        {
            PlanPrevoyance plan = new("Plan Test", TypePrimaute.Cotisations, 0.9m, 0.09m, 0.07m, 0.02m, 100, new DateOnly(2020, 1, 1));
            Assert.That(plan.TauxCotisationTotal, Is.EqualTo(0.16m));
        }
    }

    [TestFixture]
    public class BeneficiaireTests
    {
        [Test]
        public void EstAnomalie_ActifSansAvoirVieillesse_RetourneVrai()
        {
            Beneficiaire beneficiaire = new(Guid.NewGuid(), 1985, StatutAssure.Actif, 70000m, avoirVieillesse: 0m);
            Assert.That(beneficiaire.EstAnomalie(), Is.True);
        }

        [Test]
        public void EstAnomalie_PensionneSansAvoirVieillesse_RetourneFaux()
        {
            // Un pensionné a normalement liquidé son avoir : ce n'est pas une anomalie.
            Beneficiaire beneficiaire = new(Guid.NewGuid(), 1955, StatutAssure.Pensionne, 0m, avoirVieillesse: 0m);
            Assert.That(beneficiaire.EstAnomalie(), Is.False);
        }
    }
}
