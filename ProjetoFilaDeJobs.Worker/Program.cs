using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProjetoFilaDeJobs.Worker.Consumers;
using ProjetoFilaDeJobs.Worker.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<WorkerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.AddConsumer<PedidoCriadoConsumer>();

    busConfigurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("admin");
            h.Password("admin123");
        });

        // Retry: se o Consumer lançar exceção, tenta de novo automaticamente.
        // Precisa vir ANTES de ConfigureEndpoints, pois se aplica a tudo
        // que for configurado depois dele.
        cfg.UseMessageRetry(retryConfig =>
        {
            retryConfig.Interval(3, TimeSpan.FromSeconds(5));
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();