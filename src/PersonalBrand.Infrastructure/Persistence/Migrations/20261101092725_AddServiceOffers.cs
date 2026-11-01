using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalBrand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceOffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    PriceLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IconName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOffers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_IsFeatured",
                table: "ServiceOffers",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_IsPublished",
                table: "ServiceOffers",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_Slug",
                table: "ServiceOffers",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_UserId_DisplayOrder",
                table: "ServiceOffers",
                columns: new[] { "UserId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceOffers");
        }
    }
}
