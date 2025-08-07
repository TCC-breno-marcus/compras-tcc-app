using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pessoas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CPF = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pessoas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CatMat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LinkImagem = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Especificacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PrecoSugerido = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CategoriaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Servidores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PessoaId = table.Column<long>(type: "bigint", nullable: false),
                    IdentificadorInterno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsGestor = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servidores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servidores_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Gestores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServidorId = table.Column<long>(type: "bigint", nullable: false),
                    DataUltimaSolicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gestores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gestores_Servidores_ServidorId",
                        column: x => x.ServidorId,
                        principalTable: "Servidores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solicitantes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServidorId = table.Column<long>(type: "bigint", nullable: false),
                    DataUltimaSolicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Unidade = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitantes_Servidores_ServidorId",
                        column: x => x.ServidorId,
                        principalTable: "Servidores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solicitacoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SolicitanteId = table.Column<long>(type: "bigint", nullable: false),
                    GestorId = table.Column<long>(type: "bigint", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoSolicitacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JustificativaGeral = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_Gestores_GestorId",
                        column: x => x.GestorId,
                        principalTable: "Gestores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_Solicitantes_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Solicitantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitacaoItens",
                columns: table => new
                {
                    SolicitacaoId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Justificativa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SolicitacaoGeralId = table.Column<long>(type: "bigint", nullable: true),
                    SolicitacaoPatrimonialId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacaoItens", x => new { x.SolicitacaoId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_SolicitacaoItens_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitacaoItens_Solicitacoes_SolicitacaoGeralId",
                        column: x => x.SolicitacaoGeralId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitacaoItens_Solicitacoes_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitacaoItens_Solicitacoes_SolicitacaoPatrimonialId",
                        column: x => x.SolicitacaoPatrimonialId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nome",
                table: "Categorias",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gestores_ServidorId",
                table: "Gestores",
                column: "ServidorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoriaId",
                table: "Items",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Servidores_PessoaId",
                table: "Servidores",
                column: "PessoaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacaoItens_ItemId",
                table: "SolicitacaoItens",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacaoItens_SolicitacaoGeralId",
                table: "SolicitacaoItens",
                column: "SolicitacaoGeralId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacaoItens_SolicitacaoPatrimonialId",
                table: "SolicitacaoItens",
                column: "SolicitacaoPatrimonialId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_GestorId",
                table: "Solicitacoes",
                column: "GestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_SolicitanteId",
                table: "Solicitacoes",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitantes_ServidorId",
                table: "Solicitantes",
                column: "ServidorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitacaoItens");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Solicitacoes");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Gestores");

            migrationBuilder.DropTable(
                name: "Solicitantes");

            migrationBuilder.DropTable(
                name: "Servidores");

            migrationBuilder.DropTable(
                name: "Pessoas");
        }
    }
}
