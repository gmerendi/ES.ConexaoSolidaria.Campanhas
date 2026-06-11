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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campanha",
                schema: "operacao");
        }
    }
}
