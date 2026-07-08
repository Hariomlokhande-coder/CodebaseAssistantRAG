using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodebaseAssistant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRepositoryUploadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Repositories",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SizeInBytes",
                table: "Repositories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "Repositories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "SizeInBytes",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "Repositories");
        }
    }
}
