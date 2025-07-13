using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CatMat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric", nullable: false),
                    UnidadeMedida = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
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
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pessoas", x => x.Id);
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
                    JustificativaGeral = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
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
                name: "ItemSolicitacao",
                columns: table => new
                {
                    ItemsId = table.Column<long>(type: "bigint", nullable: false),
                    SolicitacoesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSolicitacao", x => new { x.ItemsId, x.SolicitacoesId });
                    table.ForeignKey(
                        name: "FK_ItemSolicitacao_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemSolicitacao_Solicitacoes_SolicitacoesId",
                        column: x => x.SolicitacoesId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gestores_ServidorId",
                table: "Gestores",
                column: "ServidorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSolicitacao_SolicitacoesId",
                table: "ItemSolicitacao",
                column: "SolicitacoesId");

            migrationBuilder.CreateIndex(
                name: "IX_Servidores_PessoaId",
                table: "Servidores",
                column: "PessoaId",
                unique: true);

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
                name: "ItemSolicitacao");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Solicitacoes");

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
