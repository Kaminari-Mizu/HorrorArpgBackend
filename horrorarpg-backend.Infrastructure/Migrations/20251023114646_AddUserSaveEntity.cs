using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace horrorarpg_backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSaveEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSaves",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Health = table.Column<float>(type: "real", nullable: false),
                    Mana = table.Column<float>(type: "real", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SceneName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
                    PositionZ = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSaves", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSaves_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSaves");
        }
    }
}
