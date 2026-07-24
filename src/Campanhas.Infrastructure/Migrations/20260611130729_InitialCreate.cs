using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campanhas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "operacao");

            migrationBuilder.CreateTable(
                name: "campanha",
                schema: "operacao",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    meta_financeira = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_arrecadado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_campanha = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    modificado_por = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                    
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campanha", x => x.guid);
                });



            migrationBuilder.CreateTable(
                name: "doacao",
                schema: "operacao",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    guid_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_usuario = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    email_usuario = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    cpf_usuario = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false),
                    guid_campanha = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo_campanha = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    valor_doacao = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    criado_por = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modificado_por = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    correlation_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    status_doacao = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doacao", x => x.guid);

                    table.ForeignKey(
                        name: "FK_doacao_campanha",
                        column: x => x.guid_campanha,
                        principalSchema: "operacao",
                        principalTable: "campanha",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_doacao_guid_campanha",
                schema: "operacao",
                table: "doacao",
                column: "guid_campanha");

            migrationBuilder.CreateIndex(
                name: "IX_doacao_guid_usuario",
                schema: "operacao",
                table: "doacao",
                column: "guid_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campanha",
                schema: "operacao");

            migrationBuilder.DropTable(
               name: "doacao",
               schema: "operacao");
        }
    }
}
