using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fase3ConsistenciaEPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrdensServico_ClienteId",
                table: "OrdensServico");

            migrationBuilder.DropIndex(
                name: "IX_HistoricoOrdensServico_OrdemServicoId",
                table: "HistoricoOrdensServico");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_ClienteId_CriadaEm",
                table: "OrdensServico",
                columns: new[] { "ClienteId", "CriadaEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_MecanicoId",
                table: "OrdensServico",
                column: "MecanicoId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orcamentos_Versao",
                table: "Orcamentos",
                sql: "\"Versao\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_ClienteId_CriadaEm",
                table: "Notificacoes",
                columns: new[] { "ClienteId", "CriadaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_OrdemServicoId",
                table: "MovimentacoesEstoque",
                column: "OrdemServicoId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MovimentacoesEstoque_Quantidade",
                table: "MovimentacoesEstoque",
                sql: "\"Quantidade\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_ItensOrcamento_EstoqueItemId",
                table: "ItensOrcamento",
                column: "EstoqueItemId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ItensOrcamento_Quantidade",
                table: "ItensOrcamento",
                sql: "\"Quantidade\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ItensOrcamento_ValorUnitario",
                table: "ItensOrcamento",
                sql: "\"ValorUnitario\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoOrdensServico_OrdemServicoId_CriadoEm",
                table: "HistoricoOrdensServico",
                columns: new[] { "OrdemServicoId", "CriadoEm" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_EstoqueItens_PrecoUnitario",
                table: "EstoqueItens",
                sql: "\"PrecoUnitario\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EstoqueItens_QuantidadeDisponivel",
                table: "EstoqueItens",
                sql: "\"QuantidadeDisponivel\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clientes_Documento_Tamanho",
                table: "Clientes",
                sql: "char_length(\"Documento\") IN (11, 14)");

            migrationBuilder.AddForeignKey(
                name: "FK_Notificacoes_Clientes_ClienteId",
                table: "Notificacoes",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificacoes_Clientes_ClienteId",
                table: "Notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_OrdensServico_ClienteId_CriadaEm",
                table: "OrdensServico");

            migrationBuilder.DropIndex(
                name: "IX_OrdensServico_MecanicoId",
                table: "OrdensServico");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orcamentos_Versao",
                table: "Orcamentos");

            migrationBuilder.DropIndex(
                name: "IX_Notificacoes_ClienteId_CriadaEm",
                table: "Notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_OrdemServicoId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MovimentacoesEstoque_Quantidade",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropIndex(
                name: "IX_ItensOrcamento_EstoqueItemId",
                table: "ItensOrcamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ItensOrcamento_Quantidade",
                table: "ItensOrcamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ItensOrcamento_ValorUnitario",
                table: "ItensOrcamento");

            migrationBuilder.DropIndex(
                name: "IX_HistoricoOrdensServico_OrdemServicoId_CriadoEm",
                table: "HistoricoOrdensServico");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EstoqueItens_PrecoUnitario",
                table: "EstoqueItens");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EstoqueItens_QuantidadeDisponivel",
                table: "EstoqueItens");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clientes_Documento_Tamanho",
                table: "Clientes");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_ClienteId",
                table: "OrdensServico",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoOrdensServico_OrdemServicoId",
                table: "HistoricoOrdensServico",
                column: "OrdemServicoId");
        }
    }
}
