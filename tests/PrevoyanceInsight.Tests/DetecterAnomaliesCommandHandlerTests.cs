namespace PrevoyanceInsight.Tests
{
    using Moq;
    using NUnit.Framework;
    using PrevoyanceInsight.Application.Common;
    using PrevoyanceInsight.Application.Plans.Commands;
    using PrevoyanceInsight.Domain.Entities;

    [TestFixture]
    public class DetecterAnomaliesCommandHandlerTests
    {
        private Mock<IPlanRepository> repository = null!;
        private Mock<IEventPublisher> eventPublisher = null!;
        private DetecterAnomaliesCommandHandler handler = null!;
        private readonly Guid planId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            this.repository = new Mock<IPlanRepository>();
            this.eventPublisher = new Mock<IEventPublisher>();
            this.handler = new DetecterAnomaliesCommandHandler(this.repository.Object, this.eventPublisher.Object);
        }

        [Test]
        public async Task Handle_DetecteActifAvecAvoirNul_PublieUnEvenementParAnomalie()
        {
            List<Beneficiaire> beneficiaires =
            [
                new(this.planId, 1985, StatutAssure.Actif, 80000m, avoirVieillesse: 0m), // anomalie
                new(this.planId, 1970, StatutAssure.Pensionne, 0m, avoirVieillesse: 0m), // normal : pensionné
                new(this.planId, 1990, StatutAssure.Actif, 65000m, avoirVieillesse: 45000m)
            ];
            this.repository.Setup(r => r.ObtenirBeneficiairesAsync(this.planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(beneficiaires);

            RapportAnomalies resultat = await this.handler.Handle(new DetecterAnomaliesCommand(this.planId), CancellationToken.None);

            Assert.That(resultat.NombreAnomalies, Is.EqualTo(1));
            this.eventPublisher.Verify(
                p => p.PublierAsync(It.IsAny<AnomalieDetecteeEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_AucuneAnomalie_NePublieAucunEvenement()
        {
            List<Beneficiaire> beneficiaires = [new(this.planId, 1990, StatutAssure.Actif, 65000m, avoirVieillesse: 45000m)];
            this.repository.Setup(r => r.ObtenirBeneficiairesAsync(this.planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(beneficiaires);

            RapportAnomalies resultat = await this.handler.Handle(new DetecterAnomaliesCommand(this.planId), CancellationToken.None);

            Assert.That(resultat.NombreAnomalies, Is.EqualTo(0));
            this.eventPublisher.Verify(
                p => p.PublierAsync(It.IsAny<AnomalieDetecteeEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
