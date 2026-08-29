using MassTransit;
using Microsoft.EntityFrameworkCore; // necessário para o método UseNpgsql funcionar (extensão do pacote Npgsql)
using ProjetoFilaDeJobs.Data; // necessário para o compilador enxergar a classe AppDbContext
using ProjetoFilaDeJobs.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IPedidoService, PedidoService>();
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

// ... (outros usings e registros já existentes)

// Registra o MassTransit no container de DI e configura o RabbitMQ como transporte.
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.UsingRabbitMq((context, cfg) =>
    {
        // Host aponta para o container que subimos via docker-compose.
        // "/" é o vhost padrão do RabbitMQ; admin/admin123 são as credenciais
        // que definimos em RABBITMQ_DEFAULT_USER / RABBITMQ_DEFAULT_PASS.
        cfg.Host("localhost", "/", h =>
        {
            h.Username("admin");
            h.Password("admin123");
        });

        // Configura automaticamente os endpoints com base nos Consumers
        // registrados no container - ainda não temos nenhum Consumer,
        // então por enquanto isso não tem efeito prático, mas já deixamos pronto.
        cfg.ConfigureEndpoints(context);
    });
});

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