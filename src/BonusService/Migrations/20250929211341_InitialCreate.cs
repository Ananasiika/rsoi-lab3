using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BonusService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "privilege",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, defaultValue: "BRONZE"),
                    balance = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privilege", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "privilege_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    privilege_id = table.Column<int>(type: "integer", nullable: false),
                    ticket_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    balance_diff = table.Column<int>(type: "integer", nullable: false),
                    operation_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privilege_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_privilege_history_privilege_privilege_id",
                        column: x => x.privilege_id,
                        principalTable: "privilege",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_privilege_username",
                table: "privilege",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_privilege_history_privilege_id",
                table: "privilege_history",
                column: "privilege_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "privilege_history");

            migrationBuilder.DropTable(
                name: "privilege");
        }
    }
}