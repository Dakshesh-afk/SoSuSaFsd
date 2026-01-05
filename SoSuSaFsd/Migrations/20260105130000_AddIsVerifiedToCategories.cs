using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoSuSaFsd.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVerifiedToCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Categories");
        }
    }
}

