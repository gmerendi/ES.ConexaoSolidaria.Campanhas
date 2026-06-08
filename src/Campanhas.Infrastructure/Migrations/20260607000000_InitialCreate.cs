using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campanhas.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Campanhas",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DataModificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                MetaFinanceira = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                ValorArrecadado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                StatusCampanha = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Campanhas", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Campanhas");
    }
}
