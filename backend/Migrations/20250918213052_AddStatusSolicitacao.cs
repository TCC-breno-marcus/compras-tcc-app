using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusSolicitacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Solicitacoes",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StatusSolicitacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusSolicitacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoSolicitacoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SolicitacaoId = table.Column<long>(type: "bigint", nullable: false),
                    DataOcorrencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StatusAnteriorId = table.Column<int>(type: "integer", nullable: false),
                    StatusNovoId = table.Column<int>(type: "integer", nullable: false),
                    PessoaId = table.Column<long>(type: "bigint", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoSolicitacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoSolicitacoes_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricoSolicitacoes_Solicitacoes_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoSolicitacoes_StatusSolicitacoes_StatusAnteriorId",
                        column: x => x.StatusAnteriorId,
                        principalTable: "StatusSolicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricoSolicitacoes_StatusSolicitacoes_StatusNovoId",
                        column: x => x.StatusNovoId,
                        principalTable: "StatusSolicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "StatusSolicitacoes",
                columns: new[] { "Id", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, "Solicitação recém-criada, aguardando a análise do gestor.", "Pendente" },
                    { 2, "Devolvida ao solicitante para correção ou mais informações.", "Aguardando Ajustes" },
                    { 3, "A solicitação foi aceita pelo gestor e seguirá para o próximo fluxo.", "Aprovada" },
                    { 4, "O pedido foi permanentemente negado pelo gestor.", "Rejeitada" },
                    { 5, "Encerrada antecipadamente pelo solicitante ou gestor.", "Cancelada" },
                    { 6, "Estado de arquivamento para solicitações de ciclos anteriores.", "Encerrada" }
                });

  	    migrationBuilder.Sql("UPDATE \"Solicitacoes\" SET \"StatusId\" = 1;");

	    migrationBuilder.AlterColumn<int>(
        	name: "StatusId",
        	table: "Solicitacoes",
        	type: "integer",
        	nullable: false,
        	defaultValue: 1); 

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_StatusId",
                table: "Solicitacoes",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoSolicitacoes_PessoaId",
                table: "HistoricoSolicitacoes",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoSolicitacoes_SolicitacaoId",
                table: "HistoricoSolicitacoes",
                column: "SolicitacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoSolicitacoes_StatusAnteriorId",
                table: "HistoricoSolicitacoes",
                column: "StatusAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoSolicitacoes_StatusNovoId",
                table: "HistoricoSolicitacoes",
                column: "StatusNovoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitacoes_StatusSolicitacoes_StatusId",
                table: "Solicitacoes",
                column: "StatusId",
                principalTable: "StatusSolicitacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitacoes_StatusSolicitacoes_StatusId",
                table: "Solicitacoes");

            migrationBuilder.DropTable(
                name: "HistoricoSolicitacoes");

            migrationBuilder.DropTable(
                name: "StatusSolicitacoes");

            migrationBuilder.DropIndex(
                name: "IX_Solicitacoes_StatusId",
                table: "Solicitacoes");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Solicitacoes");
        }
    }
}
