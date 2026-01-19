using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrunchyRolls.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserConsents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsentPrivacyPolicy = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentMarketing = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentCookies = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentTermsConditions = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentDataProcessing = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PrivacyPolicyVersion = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true, defaultValue: "1.0"),
                    DataExportRequested = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataDeletionRequested = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataDeletionRequestedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConsents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserId",
                table: "UserConsents",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_ConsentDate",
                table: "UserConsents",
                column: "ConsentDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserConsents");
        }
    }
}