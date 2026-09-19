using Microsoft.EntityFrameworkCore;
using PrevoyanceInsight.Domain.Entities;

namespace PrevoyanceInsight.Infrastructure.Persistence
{
    /// <summary>
    /// Contexte EF Core pointant sur PostgreSQL. Les tables sont alimentées en
    /// continu par un pipeline CDC (Debezium + Kafka/RabbitMQ typiquement) en
    /// provenance des systèmes source de gestion — voir docs/ARCHITECTURE.md.
    /// </summary>
    public class PrevoyanceDbContext(DbContextOptions<PrevoyanceDbContext> options) : DbContext(options)
    {
        public DbSet<PlanPrevoyance> Plans => this.Set<PlanPrevoyance>();
        public DbSet<Beneficiaire> Beneficiaires => this.Set<Beneficiaire>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlanPrevoyance>(entity =>
            {
                entity.ToTable("plans_prevoyance");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nom).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Primaute).HasConversion<string>();
                entity.Property(p => p.TauxCouverture).HasColumnType("numeric(6,4)");
                entity.Property(p => p.TauxCotisationEmployeur).HasColumnType("numeric(6,4)");
                entity.Property(p => p.TauxCotisationEmploye).HasColumnType("numeric(6,4)");
                entity.Property(p => p.TauxTechnique).HasColumnType("numeric(6,4)");
            });

            modelBuilder.Entity<Beneficiaire>(entity =>
            {
                entity.ToTable("beneficiaires");
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Statut).HasConversion<string>();
                entity.Property(b => b.SalaireAssure).HasColumnType("numeric(12,2)");
                entity.Property(b => b.AvoirVieillesse).HasColumnType("numeric(14,2)");
                entity.HasIndex(b => b.PlanId); // requis pour les contrôles de masse et les stats
            });
        }
    }
}
