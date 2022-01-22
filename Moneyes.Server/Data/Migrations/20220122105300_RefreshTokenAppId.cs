using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneyes.Server.Data.Migrations
{
    public partial class RefreshTokenAppId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppId",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppId",
                table: "RefreshTokens");
        }
    }
}
