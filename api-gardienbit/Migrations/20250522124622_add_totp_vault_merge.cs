using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_gardienbit.Migrations
{
    /// <inheritdoc />
    public partial class add_totp_vault_merge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    CliId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CliName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CliEntraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.CliId);
                });

            migrationBuilder.CreateTable(
                name: "LogActions",
                columns: table => new
                {
                    LoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogActions", x => x.LoaId);
                });

            migrationBuilder.CreateTable(
                name: "Vaults",
                columns: table => new
                {
                    VauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VauName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VauHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    VauFavorite = table.Column<bool>(type: "bit", nullable: false),
                    VauSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    VauTOTP = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    VauEntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VauLastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VauClientCliId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaults", x => x.VauId);
                    table.ForeignKey(
                        name: "FK_Vaults_Clients_VauClientCliId",
                        column: x => x.VauClientCliId,
                        principalTable: "Clients",
                        principalColumn: "CliId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogCliEntraId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogCliId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogVauId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogVauName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogPwpId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogPwpName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogEntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogActionLoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_Logs_LogActions_LogActionLoaId",
                        column: x => x.LogActionLoaId,
                        principalTable: "LogActions",
                        principalColumn: "LoaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CatEntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CatVaultVauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CatId);
                    table.ForeignKey(
                        name: "FK_Categories_Vaults_CatVaultVauId",
                        column: x => x.CatVaultVauId,
                        principalTable: "Vaults",
                        principalColumn: "VauId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VaultSessions",
                columns: table => new
                {
                    VasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VasVaultVauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VasClientCliId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VasIsTotpResolved = table.Column<bool>(type: "bit", nullable: false),
                    VasPrivateKey = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    VasPublicKey = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    VasEncryptionKeyPrivate = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    VasEncryptionKeyPublic = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    VasEntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VasLastActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultSessions", x => x.VasId);
                    table.ForeignKey(
                        name: "FK_VaultSessions_Clients_VasClientCliId",
                        column: x => x.VasClientCliId,
                        principalTable: "Clients",
                        principalColumn: "CliId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VaultSessions_Vaults_VasVaultVauId",
                        column: x => x.VasVaultVauId,
                        principalTable: "Vaults",
                        principalColumn: "VauId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VaultUserAccess",
                columns: table => new
                {
                    VaultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<int>(type: "int", nullable: false),
                    NbUsed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultUserAccess", x => new { x.VaultId, x.UserId });
                    table.ForeignKey(
                        name: "FK_VaultUserAccess_Vaults_VaultId",
                        column: x => x.VaultId,
                        principalTable: "Vaults",
                        principalColumn: "VauId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VaultUserLinks",
                columns: table => new
                {
                    VaultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessGrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultUserLinks", x => new { x.VaultId, x.UserId });
                    table.ForeignKey(
                        name: "FK_VaultUserLinks_Clients_UserId",
                        column: x => x.UserId,
                        principalTable: "Clients",
                        principalColumn: "CliId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VaultUserLinks_Vaults_VaultId",
                        column: x => x.VaultId,
                        principalTable: "Vaults",
                        principalColumn: "VauId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PwdPackages",
                columns: table => new
                {
                    PwpId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PwpName_EnfCipherText = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpName_EnfAuthTag = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpName_EnfInitialisationVector = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpContent_EnfCipherText = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpContent_EnfAuthTag = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpContent_EnfInitialisationVector = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpUrl_EnfCipherText = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpUrl_EnfAuthTag = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpUrl_EnfInitialisationVector = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpCom_EnfCipherText = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpCom_EnfAuthTag = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpCom_EnfInitialisationVector = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PwpEntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PwpLastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PwPVaultVauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PwpCategoryCatId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PwdPackages", x => x.PwpId);
                    table.ForeignKey(
                        name: "FK_PwdPackages_Categories_PwpCategoryCatId",
                        column: x => x.PwpCategoryCatId,
                        principalTable: "Categories",
                        principalColumn: "CatId");
                    table.ForeignKey(
                        name: "FK_PwdPackages_Vaults_PwPVaultVauId",
                        column: x => x.PwPVaultVauId,
                        principalTable: "Vaults",
                        principalColumn: "VauId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CatVaultVauId",
                table: "Categories",
                column: "CatVaultVauId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_LogActionLoaId",
                table: "Logs",
                column: "LogActionLoaId");

            migrationBuilder.CreateIndex(
                name: "IX_PwdPackages_PwpCategoryCatId",
                table: "PwdPackages",
                column: "PwpCategoryCatId");

            migrationBuilder.CreateIndex(
                name: "IX_PwdPackages_PwPVaultVauId",
                table: "PwdPackages",
                column: "PwPVaultVauId");

            migrationBuilder.CreateIndex(
                name: "IX_Vaults_VauClientCliId",
                table: "Vaults",
                column: "VauClientCliId");

            migrationBuilder.CreateIndex(
                name: "IX_VaultSessions_VasClientCliId",
                table: "VaultSessions",
                column: "VasClientCliId");

            migrationBuilder.CreateIndex(
                name: "IX_VaultSessions_VasVaultVauId",
                table: "VaultSessions",
                column: "VasVaultVauId");

            migrationBuilder.CreateIndex(
                name: "IX_VaultUserLinks_UserId",
                table: "VaultUserLinks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "PwdPackages");

            migrationBuilder.DropTable(
                name: "VaultSessions");

            migrationBuilder.DropTable(
                name: "VaultUserAccess");

            migrationBuilder.DropTable(
                name: "VaultUserLinks");

            migrationBuilder.DropTable(
                name: "LogActions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Vaults");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
