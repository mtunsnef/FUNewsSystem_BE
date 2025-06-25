using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNewsSystem.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorFieldsToSystemAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is2FAEnabled",
                table: "SystemAccount",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Temp2FASecretKey",
                table: "SystemAccount",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecretKey",
                table: "SystemAccount",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is2FAEnabled",
                table: "SystemAccount");

            migrationBuilder.DropColumn(
                name: "Temp2FASecretKey",
                table: "SystemAccount");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecretKey",
                table: "SystemAccount");
        }
    }
}
