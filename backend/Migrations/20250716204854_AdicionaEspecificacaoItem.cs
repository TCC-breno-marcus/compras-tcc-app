using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaEspecificacaoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantidade",
                table: "Items");

            migrationBuilder.AddColumn<string>(
                name: "Especificacao",
                table: "Items",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Especificacao",
                table: "Items");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantidade",
                table: "Items",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
