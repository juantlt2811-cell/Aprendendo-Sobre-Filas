using ProjetoFilaDeJobs.Data; // necessário para o compilador enxergar a classe AppDbContext
using Microsoft.EntityFrameworkCore; // necessário para o método UseNpgsql funcionar (extensão do pacote Npgsql)

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Registra o AppDbContext no container de Dependency Injection.
// A partir daqui, qualquer Controller ou Service pode receber "AppDbContext context"
// no construtor, que o próprio ASP.NET Core injeta automaticamente.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// GetConnectionString("DefaultConnection") busca a string configurada via User Secrets
// (em Development) sob a chave "ConnectionStrings:DefaultConnection".
// UseNpgsql diz ao EF Core: "traduza os comandos para o dialeto SQL do PostgreSQL".

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();