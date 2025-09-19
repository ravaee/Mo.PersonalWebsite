using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mo.PersonalWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishedAtToArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Articles",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Articles");
        }
    }
}
