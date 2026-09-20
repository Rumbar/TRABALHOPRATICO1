using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LocadoraVeiculos.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PercentualAjuste = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.CategoriaId);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Cpf = table.Column<string>(type: "nchar(11)", fixedLength: true, maxLength: 11, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "date", nullable: true),
                    NumeroCnh = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    CategoriaCnh = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.ClienteId);
                });

            migrationBuilder.CreateTable(
                name: "Fabricante",
                columns: table => new
                {
                    FabricanteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PaisOrigem = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    AnoFundacao = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fabricante", x => x.FabricanteId);
                });

            migrationBuilder.CreateTable(
                name: "Funcionario",
                columns: table => new
                {
                    FuncionarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Cpf = table.Column<string>(type: "nchar(11)", fixedLength: true, maxLength: 11, nullable: false),
                    Matricula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    DataAdmissao = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionario", x => x.FuncionarioId);
                });

            migrationBuilder.CreateTable(
                name: "Veiculo",
                columns: table => new
                {
                    VeiculoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Placa = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    AnoFabricacao = table.Column<int>(type: "int", nullable: false),
                    Quilometragem = table.Column<int>(type: "int", nullable: false),
                    Cor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ValorDiaria = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    FabricanteId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculo", x => x.VeiculoId);
                    table.CheckConstraint("CK_Veiculo_AnoFabricacao", "[AnoFabricacao] >= 1950");
                    table.CheckConstraint("CK_Veiculo_Quilometragem", "[Quilometragem] >= 0");
                    table.CheckConstraint("CK_Veiculo_ValorDiaria", "[ValorDiaria] > 0");
                    table.ForeignKey(
                        name: "FK_Veiculo_Categoria",
                        column: x => x.CategoriaId,
                        principalTable: "Categoria",
                        principalColumn: "CategoriaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Veiculo_Fabricante",
                        column: x => x.FabricanteId,
                        principalTable: "Fabricante",
                        principalColumn: "FabricanteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Aluguel",
                columns: table => new
                {
                    AluguelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    VeiculoId = table.Column<int>(type: "int", nullable: false),
                    FuncionarioId = table.Column<int>(type: "int", nullable: false),
                    DataRetirada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QuilometragemInicial = table.Column<int>(type: "int", nullable: false),
                    QuilometragemFinal = table.Column<int>(type: "int", nullable: true),
                    ValorDiaria = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Observacao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aluguel", x => x.AluguelId);
                    table.CheckConstraint("CK_Aluguel_Periodo", "[DataPrevistaDevolucao] >= [DataRetirada]");
                    table.CheckConstraint("CK_Aluguel_Quilometragem", "[QuilometragemFinal] IS NULL OR [QuilometragemFinal] >= [QuilometragemInicial]");
                    table.CheckConstraint("CK_Aluguel_ValorDiaria", "[ValorDiaria] > 0");
                    table.ForeignKey(
                        name: "FK_Aluguel_Cliente",
                        column: x => x.ClienteId,
                        principalTable: "Cliente",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aluguel_Funcionario",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionario",
                        principalColumn: "FuncionarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aluguel_Veiculo",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculo",
                        principalColumn: "VeiculoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categoria",
                columns: new[] { "CategoriaId", "Descricao", "Nome", "PercentualAjuste" },
                values: new object[,]
                {
                    { 1, "Veiculos de entrada, motor 1.0", "Economico", 0m },
                    { 2, "Sedans compactos e hatches medios", "Intermediario", 15m },
                    { 3, "Utilitarios esportivos", "SUV", 35m },
                    { 4, "Veiculos de luxo e alto padrao", "Executivo", 60m }
                });

            migrationBuilder.InsertData(
                table: "Fabricante",
                columns: new[] { "FabricanteId", "AnoFundacao", "Nome", "PaisOrigem" },
                values: new object[,]
                {
                    { 1, 1937, "Volkswagen", "Alemanha" },
                    { 2, 1899, "Fiat", "Italia" },
                    { 3, 1937, "Toyota", "Japao" },
                    { 4, 1911, "Chevrolet", "Estados Unidos" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aluguel_ClienteId",
                table: "Aluguel",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Aluguel_DataRetirada",
                table: "Aluguel",
                column: "DataRetirada");

            migrationBuilder.CreateIndex(
                name: "IX_Aluguel_FuncionarioId",
                table: "Aluguel",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Aluguel_VeiculoId",
                table: "Aluguel",
                column: "VeiculoId");

            migrationBuilder.CreateIndex(
                name: "UQ_Categoria_Nome",
                table: "Categoria",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Cliente_Cpf",
                table: "Cliente",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Cliente_Email",
                table: "Cliente",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Fabricante_Nome",
                table: "Fabricante",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Funcionario_Cpf",
                table: "Funcionario",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Funcionario_Matricula",
                table: "Funcionario",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veiculo_CategoriaId",
                table: "Veiculo",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculo_FabricanteId",
                table: "Veiculo",
                column: "FabricanteId");

            migrationBuilder.CreateIndex(
                name: "UQ_Veiculo_Placa",
                table: "Veiculo",
                column: "Placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aluguel");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropTable(
                name: "Funcionario");

            migrationBuilder.DropTable(
                name: "Veiculo");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "Fabricante");
        }
    }
}
