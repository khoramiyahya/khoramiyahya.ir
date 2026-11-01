using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalBrand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSeoRedirects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeoRedirects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FromPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ToUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoRedirects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeoRedirects_FromPath",
                table: "SeoRedirects",
                column: "FromPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeoRedirects_IsPublished",
                table: "SeoRedirects",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_SeoRedirects_UserId",
                table: "SeoRedirects",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeoRedirects");
        }
    }
}
