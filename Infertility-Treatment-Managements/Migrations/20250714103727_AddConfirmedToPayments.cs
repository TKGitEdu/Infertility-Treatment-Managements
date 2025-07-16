using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infertility_Treatment_Managements.Migrations
{
    /// <inheritdoc />
    public partial class AddConfirmedToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Confirmed",
                table: "Payments",
                type: "boolean",
                nullable: true,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confirmed",
                table: "Payments");
        }
    }
}
