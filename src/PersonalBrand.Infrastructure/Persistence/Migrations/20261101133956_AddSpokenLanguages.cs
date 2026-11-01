using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalBrand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpokenLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpokenLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Proficiency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpokenLanguages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpokenLanguages_IsPublished",
                table: "SpokenLanguages",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_SpokenLanguages_Proficiency",
                table: "SpokenLanguages",
                column: "Proficiency");

            migrationBuilder.CreateIndex(
                name: "IX_SpokenLanguages_UserId_DisplayOrder",
                table: "SpokenLanguages",
                columns: new[] { "UserId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SpokenLanguages_UserId_Name",
                table: "SpokenLanguages",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpokenLanguages");
        }
    }
}
