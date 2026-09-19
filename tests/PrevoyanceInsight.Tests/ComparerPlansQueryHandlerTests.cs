namespace PrevoyanceInsight.Tests
{
    using Moq;
    using NUnit.Framework;
    using PrevoyanceInsight.Application.Common;
    using PrevoyanceInsight.Application.Plans.Queries;
    using PrevoyanceInsight.Domain.Entities;
    using PrevoyanceInsight.Domain.ValueObjects;

    /// <summary>
    /// Écrits avant le handler (TDD) : ces trois cas fixent le contrat attendu de
    /// ComparerPlansQueryHandler avant même l'implémentation — écart positif,
    /// négatif, et égalité, plus le cas d'erreur sur un identifiant inconnu.
    /// </summary>
    [TestFixture]
    public class ComparerPlansQueryHandlerTests
    {
        private Mock<IPlanRepository> repository = null!;
        private ComparerPlansQueryHandler handler = null!;

        [SetUp]
        public void SetUp()
        {
            this.repository = new Mock<IPlanRepository>();
            this.handler = new ComparerPlansQueryHandler(this.repository.Object);
        }

        [Test]
        public async Task Handle_QuandPlanAMeilleureCouverture_RetourneEcartPositifEtSyntheseCorrecte()
        {
            PlanPrevoyance planA = CreerPlan("Plan Cadres", tauxCouverture: 0.95m, tauxCotisation: 0.18m);
            PlanPrevoyance planB = CreerPlan("Plan Employés", tauxCouverture: 0.85m, tauxCotisation: 0.14m);
            this.ConfigurerRepository(planA, planB);

            ComparaisonPlans resultat = await this.handler.Handle(new ComparerPlansQuery(planA.Id, planB.Id), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(resultat.EcartTauxCouverture, Is.EqualTo(0.10m).Within(0.0001m));
                Assert.That(resultat.Synthese, Does.Contain("Plan Cadres"));
                Assert.That(resultat.Synthese, Does.Contain("supérieur"));
            });
        }

        [Test]
        public async Task Handle_QuandPlansIdentiques_RetourneEcartZero()
        {
            PlanPrevoyance planA = CreerPlan("Plan Unique", tauxCouverture: 0.90m, tauxCotisation: 0.16m);
            PlanPrevoyance planB = CreerPlan("Plan Unique Bis", tauxCouverture: 0.90m, tauxCotisation: 0.16m);
            this.ConfigurerRepository(planA, planB);

            ComparaisonPlans resultat = await this.handler.Handle(new ComparerPlansQuery(planA.Id, planB.Id), CancellationToken.None);

            Assert.That(resultat.EcartTauxCouverture, Is.EqualTo(0m));
        }

        [Test]
        public void Handle_QuandPlanIntrouvable_LeveKeyNotFoundException()
        {
            this.repository.Setup(r => r.ObtenirParIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PlanPrevoyance?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                                                         this.handler.Handle(new ComparerPlansQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
        }

        private void ConfigurerRepository(PlanPrevoyance planA, PlanPrevoyance planB)
        {
            this.repository.Setup(r => r.ObtenirParIdAsync(planA.Id, It.IsAny<CancellationToken>())).ReturnsAsync(planA);
            this.repository.Setup(r => r.ObtenirParIdAsync(planB.Id, It.IsAny<CancellationToken>())).ReturnsAsync(planB);
        }

        private static PlanPrevoyance CreerPlan(string nom, decimal tauxCouverture, decimal tauxCotisation) =>
            new(nom, TypePrimaute.Cotisations, tauxCouverture, tauxCotisation / 2, tauxCotisation / 2, 0.02m, 500, new DateOnly(2020, 1, 1));
    }
}
