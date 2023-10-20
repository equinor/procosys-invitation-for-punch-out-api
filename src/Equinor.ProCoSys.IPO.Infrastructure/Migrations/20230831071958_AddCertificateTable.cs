using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    PcsGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Plant = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Persons_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Persons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificates_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CertificateCommPkg",
                columns: table => new
                {
                    CertificateCommPkgsId = table.Column<int>(type: "int", nullable: false),
                    CertificateScopesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateCommPkg", x => new { x.CertificateCommPkgsId, x.CertificateScopesId });
                    table.ForeignKey(
                        name: "FK_CertificateCommPkg_Certificates_CertificateScopesId",
                        column: x => x.CertificateScopesId,
                        principalTable: "Certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CertificateCommPkg_CommPkgs_CertificateCommPkgsId",
                        column: x => x.CertificateCommPkgsId,
                        principalTable: "CommPkgs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CertificateMcPkg",
                columns: table => new
                {
                    CertificateMcPkgsId = table.Column<int>(type: "int", nullable: false),
                    CertificateScopesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateMcPkg", x => new { x.CertificateMcPkgsId, x.CertificateScopesId });
                    table.ForeignKey(
                        name: "FK_CertificateMcPkg_Certificates_CertificateScopesId",
                        column: x => x.CertificateScopesId,
                        principalTable: "Certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CertificateMcPkg_McPkgs_CertificateMcPkgsId",
                        column: x => x.CertificateMcPkgsId,
                        principalTable: "McPkgs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateCommPkg_CertificateScopesId",
                table: "CertificateCommPkg",
                column: "CertificateScopesId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateMcPkg_CertificateScopesId",
                table: "CertificateMcPkg",
                column: "CertificateScopesId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CreatedById",
                table: "Certificates",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_Plant_ProjectId_PcsGuid",
                table: "Certificates",
                columns: new[] { "Plant", "ProjectId", "PcsGuid" });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ProjectId",
                table: "Certificates",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateCommPkg");

            migrationBuilder.DropTable(
                name: "CertificateMcPkg");

            migrationBuilder.DropTable(
                name: "Certificates");
        }
    }
}
