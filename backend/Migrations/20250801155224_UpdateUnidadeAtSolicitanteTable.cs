using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUnidadeAtSolicitanteTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Unidade",
                table: "Solicitantes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Descricao", "IsActive", "Nome" },
                values: new object[,]
                {
                    { 1L, "Componentes discretos e integrados para montagem e prototipagem de circuitos. Inclui resistores, capacitores, transistores, MCUs, LEDs e PCBs.", true, "Componentes Eletrônicos" },
                    { 2L, "Equipamentos elétricos de uso doméstico, em cozinhas ou escritórios. Abrange linha branca, portáteis e aparelhos de climatização.", true, "Eletrodomésticos" },
                    { 3L, "Instrumentos manuais e elétricos para manutenção, montagem, reparos e medições. Inclui chaves de fenda, alicates, furadeiras e multímetros.", true, "Ferramentas" },
                    { 4L, "Substâncias e compostos químicos utilizados em análises e sínteses. Inclui ácidos, bases, solventes, sais e padrões analíticos.", true, "Reagentes Químicos" },
                    { 5L, "Utensílios, consumíveis e pequenos equipamentos para uso geral em laboratório que não são vidrarias ou reagentes.", true, "Materiais de Laboratório" },
                    { 6L, "Móveis para ambientes de escritório, laboratórios ou áreas comuns, como mesas, cadeiras, armários e bancadas de trabalho.", true, "Mobiliário" },
                    { 7L, "Categoria residual para itens que não se enquadram claramente em nenhuma outra classificação. Ideal para materiais de escritório ou de consumo geral.", true, "Diversos" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.AlterColumn<string>(
                name: "Unidade",
                table: "Solicitantes",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
