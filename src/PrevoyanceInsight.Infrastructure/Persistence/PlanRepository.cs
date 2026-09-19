using Microsoft.EntityFrameworkCore;
using PrevoyanceInsight.Application.Common;
using PrevoyanceInsight.Domain.Entities;

namespace PrevoyanceInsight.Infrastructure.Persistence
{
    public class PlanRepository(PrevoyanceDbContext dbContext) : IPlanRepository
    {
        public Task<PlanPrevoyance?> ObtenirParIdAsync(Guid id, CancellationToken ct = default) =>
            dbContext.Plans.FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IReadOnlyList<PlanPrevoyance>> ListerTousAsync(CancellationToken ct = default) =>
            await dbContext.Plans.AsNoTracking().OrderBy(p => p.Nom).ToListAsync(ct);

        public async Task<IReadOnlyList<Beneficiaire>> ObtenirBeneficiairesAsync(Guid planId, CancellationToken ct = default) =>
            await dbContext.Beneficiaires.AsNoTracking().Where(b => b.PlanId == planId).ToListAsync(ct);

        public async Task AjouterAsync(PlanPrevoyance plan, CancellationToken ct = default)
        {
            await dbContext.Plans.AddAsync(plan, ct);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
