using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calls_AspNetUsers_CallerId",
                table: "Calls");

            migrationBuilder.DropForeignKey(
                name: "FK_Calls_AspNetUsers_ReceiverId",
                table: "Calls");

            migrationBuilder.AlterColumn<int>(
                name: "CallerId",
                table: "Calls",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecipientId",
                table: "Calls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_AspNetUsers_CallerId",
                table: "Calls",
                column: "CallerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_AspNetUsers_ReceiverId",
                table: "Calls",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calls_AspNetUsers_CallerId",
                table: "Calls");

            migrationBuilder.DropForeignKey(
                name: "FK_Calls_AspNetUsers_ReceiverId",
                table: "Calls");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "Calls");

            migrationBuilder.AlterColumn<int>(
                name: "CallerId",
                table: "Calls",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_AspNetUsers_CallerId",
                table: "Calls",
                column: "CallerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_AspNetUsers_ReceiverId",
                table: "Calls",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
