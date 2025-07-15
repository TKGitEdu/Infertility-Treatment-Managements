using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infertility_Treatment_Managements.Migrations
{
    /// <inheritdoc />
    public partial class Update_TreatmentMedication_AllowNull_TreatmentPlanId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentMedications_TreatmentPlans_TreatmentPlanID",
                table: "TreatmentMedications");

            migrationBuilder.AlterColumn<string>(
                name: "TreatmentPlanID",
                table: "TreatmentMedications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentMedications_TreatmentPlans_TreatmentPlanID",
                table: "TreatmentMedications",
                column: "TreatmentPlanID",
                principalTable: "TreatmentPlans",
                principalColumn: "TreatmentPlanID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentMedications_TreatmentPlans_TreatmentPlanID",
                table: "TreatmentMedications");

            migrationBuilder.AlterColumn<string>(
                name: "TreatmentPlanID",
                table: "TreatmentMedications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentMedications_TreatmentPlans_TreatmentPlanID",
                table: "TreatmentMedications",
                column: "TreatmentPlanID",
                principalTable: "TreatmentPlans",
                principalColumn: "TreatmentPlanID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
