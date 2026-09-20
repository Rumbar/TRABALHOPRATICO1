using LocadoraVeiculos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Conexao com o SQL Server definida em appsettings.json
var connectionString = builder.Configuration.GetConnectionString("LocadoraConnection");

// Registro do contexto do Entity Framework
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Locadora de Veiculos - API",
        Version = "v1",
        Description = "Trabalho Pratico 1 - Sistema de aluguel de veiculos. " +
                      "Etapa 1: modelagem do banco de dados com Entity Framework e SQL Server."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Locadora de Veiculos v1");
    c.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

// Verifica se a aplicacao consegue se conectar ao banco configurado
app.MapGet("/api/status", async (ApplicationContext context) =>
{
    var conectado = await context.Database.CanConnectAsync();
    return Results.Ok(new
    {
        bancoConectado = conectado,
        banco = context.Database.GetDbConnection().Database,
        servidor = context.Database.GetDbConnection().DataSource
    });
});

// Retorna o mapeamento objeto-relacional montado pelo ApplicationContext:
// tabelas, chaves primarias e chaves estrangeiras de cada entidade.
app.MapGet("/api/modelo", (ApplicationContext context) =>
{
    var entidades = context.Model.GetEntityTypes().Select(e => new
    {
        entidade = e.ClrType.Name,
        tabela = e.GetTableName(),
        chavePrimaria = e.FindPrimaryKey()?.Properties.Select(p => p.Name),
        chavesEstrangeiras = e.GetForeignKeys().Select(fk => new
        {
            nome = fk.GetConstraintName(),
            coluna = string.Join(", ", fk.Properties.Select(p => p.Name)),
            referencia = fk.PrincipalEntityType.GetTableName()
        })
    });

    return Results.Ok(entidades);
});

app.MapControllers();

app.Run();
