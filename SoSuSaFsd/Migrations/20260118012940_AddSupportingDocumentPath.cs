using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoSuSaFsd.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportingDocumentPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.AddColumn<string>(
        name: "SupportingDocumentPath",
        table: "CategoryAccessRequests",
        type: "nvarchar(max)",
        nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        migrationBuilder.DropColumn(
        name: "SupportingDocumentPath",
        table: "CategoryAccessRequests");

        }
    }
}
