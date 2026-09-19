using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrevoyanceInsight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "beneficiaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnneeNaissance = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    SalaireAssure = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    AvoirVieillesse = table.Column<decimal>(type: "numeric(14,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_beneficiaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plans_prevoyance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Primaute = table.Column<string>(type: "text", nullable: false),
                    TauxCouverture = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    TauxCotisationEmployeur = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    TauxCotisationEmploye = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    TauxTechnique = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    NombreAssures = table.Column<int>(type: "integer", nullable: false),
                    DateEntreeVigueur = table.Column<DateOnly>(type: "date", nullable: false),
                    ReglementDocumentId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans_prevoyance", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_beneficiaires_PlanId",
                table: "beneficiaires",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beneficiaires");

            migrationBuilder.DropTable(
                name: "plans_prevoyance");
        }
    }
}
