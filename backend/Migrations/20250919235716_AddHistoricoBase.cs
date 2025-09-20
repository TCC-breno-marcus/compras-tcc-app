using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoricoBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricoSolicitacoes_Pessoas_PessoaId",
                table: "HistoricoSolicitacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoricoSolicitacoes_Solicitacoes_SolicitacaoId",
                table: "HistoricoSolicitacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoricoSolicitacoes_StatusSolicitacoes_StatusAnteriorId",
                table: "HistoricoSolicitacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoricoSolicitacoes_StatusSolicitacoes_StatusNovoId",
                table: "HistoricoSolicitacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistoricoSolicitacoes",
                table: "HistoricoSolicitacoes");

            migrationBuilder.DropIndex(
                name: "IX_HistoricoSolicitacoes_StatusAnteriorId",
                table: "HistoricoSolicitacoes");

            migrationBuilder.DropIndex(
                name: "IX_HistoricoSolicitacoes_StatusNovoId",
                table: "HistoricoSolicitacoes");

            migrationBuilder.DropColumn(
                name: "StatusAnteriorId",
                table: "HistoricoSolicitacoes");

            migrationBuilder.DropColumn(
                name: "StatusNovoId",
                table: "HistoricoSolicitacoes");

            migrationBuilder.RenameTable(
                name: "HistoricoSolicitacoes",
                newName: "HistoricoBase");

            migrationBuilder.RenameIndex(
                name: "IX_HistoricoSolicitacoes_SolicitacaoId",
                table: "HistoricoBase",
                newName: "IX_HistoricoBase_SolicitacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_HistoricoSolicitacoes_PessoaId",
                table: "HistoricoBase",
                newName: "IX_HistoricoBase_PessoaId");

            migrationBuilder.AlterColumn<long>(
                name: "SolicitacaoId",
                table: "HistoricoBase",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "Acao",
                table: "HistoricoBase",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Detalhes",
                table: "HistoricoBase",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "HistoricoBase",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ItemId",
                table: "HistoricoBase",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HistoricoBase",
                table: "HistoricoBase",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoBase_ItemId",
                table: "HistoricoBase",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricoBase_Items_ItemId",
                table: "HistoricoBase",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricoBase_Pessoas_PessoaId",
                table: "HistoricoBase",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricoBase_Solicitacoes_SolicitacaoId",
                table: "HistoricoBase",
                column: "SolicitacaoId",
                principalTable: "Solicitacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricoBase_Items_ItemId",
                table: "HistoricoBase");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoricoBase_Pessoas_PessoaId",
                table: "HistoricoBase");

            migrationBuilder.DropForeignKey(
                name: "FK_HistoricoBase_Solicitacoes_SolicitacaoId",
                table: "HistoricoBase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistoricoBase",
                table: "HistoricoBase");

            migrationBuilder.DropIndex(
                name: "IX_HistoricoBase_ItemId",
                table: "HistoricoBase");

            migrationBuilder.DropColumn(
                name: "Acao",
                table: "HistoricoBase");

            migrationBuilder.DropColumn(
                name: "Detalhes",
                table: "HistoricoBase");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "HistoricoBase");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "HistoricoBase");

            migrationBuilder.RenameTable(
                name: "HistoricoBase",
                newName: "HistoricoSolicitacoes");

            migrationBuilder.RenameIndex(
                name: "IX_HistoricoBase_SolicitacaoId",
                table: "HistoricoSolicitacoes",
                newName: "IX_HistoricoSolicitacoes_SolicitacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_HistoricoBase_PessoaId",
                table: "HistoricoSolicitacoes",
                newName: "IX_HistoricoSolicitacoes_PessoaId");

            migrationBuilder.AlterColumn<long>(
                name: "SolicitacaoId",
                table: "HistoricoSolicitacoes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusAnteriorId",
                table: "HistoricoSolicitacoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusNovoId",
                table: "HistoricoSolicitacoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HistoricoSolicitacoes",
                table: "HistoricoSolicitacoes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoSolicitacoes_StatusAnteriorId",
                table: "HistoricoSolicitacoes",
                column: "StatusAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoSolicitacoes_StatusNovoId",
                table: "HistoricoSolicitacoes",
                column: "StatusNovoId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricoSolicitacoes_Pessoas_PessoaId",
                table: "HistoricoSolicitacoes",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricoSolicitacoes_Solicitacoes_SolicitacaoId",
                table: "HistoricoSolicitacoes",
                column: "SolicitacaoId",
                principalTable: "Solicitacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricoSolicitacoes_StatusSolicitacoes_StatusAnteriorId",
                table: "HistoricoSolicitacoes",
                column: "StatusAnteriorId",
                principalTable: "StatusSolicitacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricoSolicitacoes_StatusSolicitacoes_StatusNovoId",
                table: "HistoricoSolicitacoes",
                column: "StatusNovoId",
                principalTable: "StatusSolicitacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
