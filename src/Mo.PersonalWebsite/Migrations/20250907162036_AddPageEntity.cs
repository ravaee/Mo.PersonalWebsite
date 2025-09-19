using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mo.PersonalWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddPageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetaKeywords = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    ShowInNavigation = table.Column<bool>(type: "bit", nullable: false),
                    NavigationOrder = table.Column<int>(type: "int", nullable: false),
                    NavigationText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NavigationIcon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Slug",
                table: "Pages",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pages");
        }
    }
}
