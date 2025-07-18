using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AjusteValorPrecoSugerido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValorUnitarioNaCompra",
                table: "SolicitacaoItens",
                newName: "ValorUnitario");

            migrationBuilder.RenameColumn(
                name: "ValorUnitario",
                table: "Items",
                newName: "PrecoSugerido");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValorUnitario",
                table: "SolicitacaoItens",
                newName: "ValorUnitarioNaCompra");

            migrationBuilder.RenameColumn(
                name: "PrecoSugerido",
                table: "Items",
                newName: "ValorUnitario");
        }
    }
}
