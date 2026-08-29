using MassTransit;
using ProjetoFilaDeJobs.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// Registra o MassTransit, e desta vez também registramos o Consumer -
// é essa linha (AddConsumer) que diz ao MassTransit "existe alguém
// interessado em PedidoCriado, crie uma fila para ele".
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

        // ConfigureEndpoints, com o Consumer já registrado acima, cria
        // automaticamente uma fila com um nome baseado no Consumer
        // (algo como "PedidoCriadoConsumer") e vincula ela ao exchange
        // ProjetoFilaDeJobs.Events:PedidoCriado que a API já publica.
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();